﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Azavea.Open.Common;
using Azavea.Open.DAO.Criteria;
using Azavea.Open.DAO.SQLServer;
using Azavea.Open.DAO.SQL;
using FileHelpers;
using Urban.DCP.Data.PDB;

namespace Urban.DCP.Data.Uploadable
{
    /// <summary>
    /// The basic preservation project entity
    /// </summary>
    [IgnoreFirst(1)]
    [DelimitedRecord(",")]
    public class Project
    {

        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string Id;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string Status;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string Subsidized;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string Category;
        [FieldQuoted('"', QuoteMode.OptionalForRead)]
        public string Name;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string Address;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string City;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string State;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string Zip;
        public int? TotalUnits;
        public int? MinAssistedUnits;
        public int? MaxAssistedUnits;
        public int? BuildingCount; 

        [FieldConverter(ConverterKind.Date, "MM/dd/yyyy")] 
        [FieldQuoted('"', QuoteMode.OptionalForRead)]
        public DateTime? OwnershipEffectiveDate;

        [FieldQuoted('"', QuoteMode.OptionalForRead)] public string OwnerName;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] public string OwnerType;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] public string ManagerName;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] public string ManagerType;

        [FieldConverter(ConverterKind.Date, "MM/dd/yyyy")] 
        [FieldQuoted('"', QuoteMode.OptionalForRead)]
        public DateTime? EarliestSubsidyEnd;

        [FieldConverter(ConverterKind.Date, "MM/dd/yyyy")]
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public DateTime? LatestSubsidyEnd;

        /// <summary>
        /// A display version of the Agencies who are providing subsidy
        /// </summary>
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string Agencies;
        /// <summary>
        /// A display version of the Portfolios of subsidy programs in this project
        /// </summary>
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string Portfolios;

        // The follow are Statuses and Year begun for various specific subsidy programs
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string HudFinancialStatus;
        public int? HudFinancialYear;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string HudPbraStatus;
        public int? HudPbraYear;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string LihtcStatus;
        public int? LihtcYear;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string HptfStatus;
        public int? HptfYear;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string IzAduStatus;
        public int? IzAduYear;
        
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string Ward;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string Anc; // Advisory Neighborhood Commission
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string PoliceArea;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string ClusterId;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string ClusterName;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        [FieldNotInFile] 
        public string ClusterCombo;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string CensusTract;
        public double? X;
        public double? Y;
        public double? Lat;
        public double? Lon;
        [FieldQuoted('"', QuoteMode.OptionalForRead)]
        public string StreetViewUrl;

        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string ImageUrl;

    }

    public class ProjectUploader: AbstractLoadable<Project>, ILoadable
    {
        public override UploadTypes UploadType
        {
            get { return UploadTypes.Project; }
        }

        public override void PreProcess(SqlTransaction trans, IList<Project> rows)
        {
            // Update the full version of the cluster name, when available
            foreach (var row in rows.Where(row => row.ClusterId != "" && row.ClusterName != ""))
            {
                row.ClusterCombo = row.ClusterId + ": " + row.ClusterName;
            }
        }

        /// <summary>
        /// After new data has been loaded, generate lists of unique values
        /// to be added to the filter lookup table for populating dropdown
        /// boxes.
        /// </summary>
        public override void PostProcess(SqlTransaction trans, IList<Project> rows)
        {
            var cols = _readDao.ClassMap.AllDataColsByObjAttrs;
            var ward = cols["Ward"];
            var psa = cols["PoliceArea"];
            var cluster = cols["ClusterCombo"];
            var anc = cols["Anc"];
            var census = cols["CensusTract"];
            var category = cols["Category"];

            PdbAttributesHelper._attrValDao.Delete(trans, new DaoCriteria(
                new PropertyInListExpression("AttributeName", new []
                    {
                        ward, psa, cluster, anc, census, category
                    }
                )));

            InsertUnique(trans, rows, r => r.Ward, ward);
            InsertUnique(trans, rows, r => r.PoliceArea, psa);
            InsertUnique(trans, rows, r => r.ClusterCombo, cluster);
            InsertUnique(trans, rows, r => r.Anc, anc);
            InsertUnique(trans, rows, r => r.CensusTract, census);
            InsertUnique(trans, rows, r => r.Category, category);
        }
    }
 
}
