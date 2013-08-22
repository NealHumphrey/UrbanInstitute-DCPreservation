﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Azavea.Database;
using Azavea.Open.Common;
using NUnit.Framework;

namespace Urban.DCP.Data.Tests
{
    [TestFixture]
    public class CommentTests
    {
        private static readonly FastDAO<User> _userDao =
            new FastDAO<User>(Config.GetConfig("PDP.Data"), "PDB");

        private static readonly FastDAO<Comment> _commentDao =
            new FastDAO<Comment>(Config.GetConfig("PDP.Data"), "PDB");

        private User sys;
        private User org1;
        private User org2;

        [TestFixtureSetUp]
        public void Setup()
        {
            _userDao.Truncate();
            _commentDao.Truncate();

            sys = new User
                {
                    Email = "",
                    EmailConfirmed = true,
                    Name = "SysAdmin",
                    UserName = "sys",
                    Roles = "SysAdmin,public"
                };
            org1 = new User()
                {
                    Email = "",
                    EmailConfirmed = true,
                    Name = "Orggy Org",
                    UserName = "ogre",
                    Roles = "Network,public",
                    Organization = 1
                };
            org2 = new User()
            {
                Email = "",
                EmailConfirmed = true,
                Name = "John Rambo",
                UserName = "johnrambo",
                Roles = "Network,public",
                Organization = 2
            };        
            UserHelper.Save(sys);
            UserHelper.Save(org1);

            Comment.AddComment("1a", sys, CommentAccessLevel.Network, "Network folks only", null);
            Comment.AddComment("1a", sys, CommentAccessLevel.Public, "Hello, everyone", null);
            Comment.AddComment("1a", org1, CommentAccessLevel.SameOrg, "Hello, co-workers", null);
        }

        [Test]
        public void TestSysAdminGetsAll()
        {
            var comments = Comment.GetAuthorizedComments("1a", sys);
            Assert.AreEqual(3, comments.Count(), "SysAdmin should see all comments");
        }

        [Test]
        public void TestAnonymousGetsPublicOnly()
        {
            var comments = Comment.GetAuthorizedComments("1a", null);
            Assert.AreEqual(1, comments.Count(), "Anonymous users should only see public comments");
            Assert.AreEqual("Hello, everyone", comments[0].Text, "Should have gotten the public comment");
        }

        [Test]
        public void TestOtherOrgUserSeesNetworkAndPublicOnly()
        {
            var comments = Comment.GetAuthorizedComments("1a", org2);
            
            Assert.AreEqual(2, comments.Count(), "Network users should only see public and Network comments, not org only");
            Assert.AreEqual(1, comments.Count(c => c.AccessLevel == CommentAccessLevel.Public), "Should have 1 public comment");
            Assert.AreEqual(1, comments.Count(c => c.AccessLevel == CommentAccessLevel.Network), "Should have 1 network comment");
        }

        [Test]
        public void TestOwnOrgUserGetsAll()
        {
            var comments = Comment.GetAuthorizedComments("1a", sys);
            Assert.AreEqual(3, comments.Count(), "SysAdmin should see all comments");            
        }

        [Test]
        public void AddCommentSansImage()
        {
            var c = Comment.AddComment("test", org1, CommentAccessLevel.Public, "My Comment", null);
            Assert.AreNotEqual(0, c.Id);

        }

        [Test]
        public void TestHasNoPicture()
        {
            var c = Comment.AddComment("test", org1, CommentAccessLevel.Public, "My Comment", null);
            Assert.IsFalse(c.HasPicture);            
        }

        [Test]
        public void TestHasPicture()
        {
            var image = new Bitmap(200, 200);
            var g = Graphics.FromImage(image);
            g.DrawLine(new Pen(Color.Black), 1, 1, 2, 2 );
            g.Save();
            var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            var c = Comment.AddComment("test", org1, CommentAccessLevel.Public, "My Comment", ms.ToArray());
            Assert.IsTrue(c.HasPicture);
        }

        [Test]
        public void TestAnonymousUserCannotEdit()
        {
            User user = null;
            TestUserAuthorized(user, false);
        }

        [Test]
        public void TestNonAuthorCannotEdit()
        {
            TestUserAuthorized(org2, false);    
        }

        [Test]
        public void TestAuthorCanEdit()
        {
            TestUserAuthorized(org1, true);
        }

        [Test]
        public void TestSysAdminCanEdit()
        {
            TestUserAuthorized(sys, true);
        }

        [Test]
        public void TestEditReplacesImage()
        {
            var orig = new byte[10];
            var update = new byte[42];

            var comment = Comment.AddComment("NL00x", org1, CommentAccessLevel.Public, "JOHN RAMBO", null);
            comment.Update(org1, null, update);
            var updatedComment = _commentDao.GetFirst("Id", comment.Id);
            Assert.AreEqual(42, updatedComment.Image.Length, "New image failed to save");
        }

        [Test]
        public void TestUpdateTextNotImage()
        {
            string orig = "First text",
                    upd = "Second text";
            var img = new byte[14];

            var c = Comment.AddComment("NL0001", org1, CommentAccessLevel.SameOrg, orig, img);
            c.Update(org1, upd, null);

            var updatedComment = _commentDao.GetFirst("Id", c.Id);
            Assert.AreEqual(upd, updatedComment.Text, "Text failed to save");
            Assert.AreEqual(14, updatedComment.Image.Length, "Image was improperly updated");
        }

        [Test]
        public void TestImageRemoved()
        {
            var img = new byte[14];

            var c = Comment.AddComment("NL0001", org1, CommentAccessLevel.SameOrg, null, img);
            c.Update(org1, null, null, true);

            var updatedComment = _commentDao.GetFirst("Id", c.Id);
            Assert.IsFalse(updatedComment.HasPicture, "Image was not removed");
        }

        private void TestUserAuthorized(User user, bool canEdit)
        {
            TestDelegate edit = delegate
            {
                var comment = Comment.AddComment("2x3", org1, CommentAccessLevel.Public, "JOHN RAMBO", null);
                comment.Update(user, "This may or may not be allowed", null);
            };

            if (canEdit)
            {
                Assert.DoesNotThrow(edit);
            }
            else
            {
                Assert.Throws<UnauthorizedToEditCommentException>(edit);    
            }
            
        }
    }
}
