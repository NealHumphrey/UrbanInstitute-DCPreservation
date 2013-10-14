﻿using System;
using FileHelpers;

namespace Urban.DCP.Data.Uploadable
{
    [DelimitedRecord(",")] 
    [IgnoreFirst(1)]
    public class Parcel
    {

        public string NlihcId;
        /// <summary>
        /// Parcel record Identifier
        /// </summary>
        public string Ssl;
        /// <summary>
        /// Res/Non Res, single family, etc
        /// </summary>
        public string ParcelType;
        /// <summary>
        /// Parcel owner name
        /// </summary>
        [FieldQuoted('"', QuoteMode.OptionalForRead)]
        public string OwnerName;
        /// <summary>
        /// Date when this ownership info took effect
        /// </summary>
        [FieldConverter(ConverterKind.Date, "MM/dd/yyyy")] 
        public DateTime? OwnerDate;
        /// <summary>
        /// Type of ownership
        /// </summary>
        [FieldQuoted('"', QuoteMode.OptionalForRead)]
        public string OwnerType;
        public int? Units;
        public double? X;
        public double? Y;
    }

    public class ParcelUploader: AbstractUploadable<Parcel>, IUploadable
    {
        public override UploadTypes UploadType
        {
            get { return UploadTypes.Parcel; }
        }
    }
}
