﻿using System;
using System.Collections.Generic;
using Azavea.Database;
using Azavea.Open.Common;
using Azavea.Open.DAO.Criteria;
using FileHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Urban.DCP.Data.Uploadable;

namespace Urban.DCP.Data
{
    public class CommentNotFoundException : Exception
    {
        public CommentNotFoundException() {}
        public CommentNotFoundException(string msg):base(msg){}
    }
    public class UnauthorizedToEditCommentException: Exception
    {
        public UnauthorizedToEditCommentException() { }
        public UnauthorizedToEditCommentException(string msg) : base(msg) { }
    }

    [IgnoreFirst(1)]
    [DelimitedRecord(",")]
    public class Comment
    {
        private static readonly FastDAO<Comment> _dao =
            new FastDAO<Comment>(Config.GetConfig("PDP.Data"), "PDB");

        public int Id;

        /// <summary>
        /// Property this comment is attached to
        /// </summary>
        public string NlihcId;

        /// <summary>
        /// Access level which this comment is viewable
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public CommentAccessLevel AccessLevel;

        /// <summary>
        /// If AccessLevel is set to Org mode, the org in question
        /// </summary>
        public int? AssociatedOrgId;
        public string AssociatedOrgName
        {
            get { 
                if (AssociatedOrgId.HasValue && AssociatedOrgId.Value > 0)
                {
                    var org = Organization.getOrgById(AssociatedOrgId.Value);
                    if (org != null) return org.Name;
                }
                return null; 
            }
        }
        [FieldConverter(ConverterKind.Date, "MM/dd/yyyy")]
        public DateTime Created;
        [FieldConverter(ConverterKind.Date, "MM/dd/yyyy")]
        public DateTime Modified;
        public string Username;

        /// <summary>
        /// Admin users can modify comments, this tracks who last
        /// edited the 
        /// </summary>
        public string LastEditorId;

        /// <summary>
        /// Text value of comment
        /// </summary>
        [FieldQuoted]
        public string Text;

        /// <summary>
        /// Optional Image attached to comment
        /// </summary>
        [JsonIgnore]
        [FieldNotInFile]
        public byte[] Image;

        [JsonIgnore]
        public User User
        {
            get { return UserHelper.GetUser(Username); }
        }

        [JsonIgnore]
        public User LastEditor
        {
            get { return UserHelper.GetUser(LastEditorId); }
        }

        public void Update(User user, string text, byte[] image, bool removeImage)
        {
            Update(user, text, image, removeImage, AccessLevel);    
        }

        /// <summary>
        /// Update the comment, if authorized
        /// </summary>
        /// <param name="user">Editing user</param>
        /// <param name="text">The new text.  If the text is unchanged, provide
        /// the original text.  This lets you take whatever is in the user text box</param>
        /// <param name="image">New image to use (null if no change, don't need to reupload orig) </param>
        /// <param name="removeImage">Flag to indicate the image was removed, since null image is no-op</param>
        /// <param name="level"></param>
        public void Update(User user, string text, byte[] image, bool removeImage, 
            CommentAccessLevel level)
        {
            AssertModifyAuthorization(user);
            if (removeImage) Image = new byte[0];
            
            // Only update the image if there was one passed and there is
            // no instruction to remove it.  If the image edit was a no-op,
            // it won't have been submitted and will be null, otherwise it's new
            if (image != null && !removeImage)
            {
                Image = image;
            }

            Text = text;
            LastEditorId = user.UserName;
            Modified = DateTime.Now;
            AccessLevel = level;
            AssociatedOrgId = GetAssociatedOrgId(user, level);

            _dao.Save(this);
        }

        /// <summary>
        /// Removes a comment from 
        /// </summary>
        /// <param name="user"></param>
        public void Delete(User user)
        {
            AssertModifyAuthorization(user);
            _dao.Delete(this);
        }

        private void AssertModifyAuthorization(User user)
        {
            // Comment authors or admins can edit/delete a comment
            if (user == null || (user.UserName != Username && !user.IsSysAdmin()))
            {
                throw new UnauthorizedToEditCommentException();
            }
        }

    /// <summary>
        /// Non images are stored as emtpy byte arrays, this
        /// is a convenience method for that.  Property so it
        /// can be serialized to the client
        /// </summary>
        /// <returns></returns>
        public bool HasPicture 
        {
            get { return Image != null && Image.Length > 0; }
            
        }

        /// <summary>
        /// Checks if a user is authorized to view a comment/image
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool IsAuthorizedToView(User user)
        {
            if (user == null) return AccessLevel == CommentAccessLevel.Public;
            if (user.IsSysAdmin()) return true;

            switch (AccessLevel)
            {
                case CommentAccessLevel.Public:
                    return true;
                case CommentAccessLevel.Network:
                    return user.IsNetworked();    
                case CommentAccessLevel.SameOrg:
                    return user.Organization == AssociatedOrgId;
            }
        
            return false;
        }

        public static Comment AddComment(string nlihcId, User user, 
            CommentAccessLevel level, string text, byte[] image)
        {
            var imageVal = image ?? new byte[] {};

            var created = DateTime.Now;
            var comment = new Comment
                {
                    NlihcId = nlihcId,
                    AccessLevel = level,
                    AssociatedOrgId = GetAssociatedOrgId(user, level),
                    Created = created,
                    Modified = created,
                    Username = user.UserName,
                    Text = text,
                    Image = imageVal
                };

            _dao.Insert(comment, true);
            return comment;
        }

        /// <summary>
        /// Set the comment's associated organization if the access
        /// level is restricted to a users organization
        /// </summary>
        /// <param name="user">User modifying the comment</param>
        /// <param name="level">Access Level of comment</param>
        /// <returns></returns>
        private static int? GetAssociatedOrgId(User user, CommentAccessLevel level)
        {
            return level == CommentAccessLevel.SameOrg ? user.Organization : null;
        }

        public static IList<Comment> GetAuthorizedComments(string nlihcId, User user)
        {
            var crit = new DaoCriteria();
            crit.Expressions.Add(new EqualExpression("NlihcId", nlihcId));

            // Anonymous users get only public comments
            if (user == null)
            {
                crit.Expressions.Add(new EqualExpression("AccessLevel", CommentAccessLevel.Public));
                return _dao.Get(crit);
            }

            // This kind of query is difficult in FastDAO, so, expecting that the number
            // of comments on a given property will be reasonable, prune off unauthorized
            // comments from the entire property comment list
            var comments = _dao.Get(crit);

            // SysAdmins can see everything
            if (user.IsSysAdmin())
            {
                return comments;
            }

            var authComments = new List<Comment>();
            foreach (var comment in comments)
            {
                switch (comment.AccessLevel)
                {
                    case CommentAccessLevel.Public:
                        authComments.Add(comment);
                        break;
                    case CommentAccessLevel.Network:
                        if (user.IsNetworked()) authComments.Add(comment);
                        break;
                    case CommentAccessLevel.SameOrg:
                        if (user.Organization == comment.AssociatedOrgId) authComments.Add(comment);
                        break;
                }
            }

            return authComments;
        }

        /// <summary>
        /// Returns a comment by its Id
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns>Comment or null if not found</returns>
        public static Comment ById(int commentId)
        {
            var user = _dao.GetFirst("Id", commentId);
            if (user == null) throw new CommentNotFoundException();
            return user;
        }
    }

    public class CommentExporter : AbstractLoadable<Comment>, ILoadable
    {
        public CommentExporter()
        {
            ReadOnly = true;
        }

        public override UploadTypes UploadType
        {
            get { return UploadTypes.Comment; }
        }

    }
}
