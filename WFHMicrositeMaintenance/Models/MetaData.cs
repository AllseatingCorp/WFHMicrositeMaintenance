using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WFHMicrositeMaintenance.Models
{
    [ModelMetadataType(typeof(UserMetadata))]
    public partial class User
    {
        public class UserMetadata
        {
            [Display(Name = "Product")]
            [Required]
            public int ProductId { get; set; }

            [Display(Name = "Email Address")]
            [Required]
            public string EmailAddress { get; set; }

            [Display(Name = "Language")]
            public string Language { get; set; }

            [Display(Name = "PIN")]
            public string Pin { get; set; }

            [Display(Name = "Street Name")]
            public string Address1 { get; set; }

            [Display(Name = "Apt./House No.")]
            public string Address2 { get; set; }

            [Display(Name = "City")]
            public string City { get; set; }

            [Display(Name = "Province")]
            public string ProvinceState { get; set; }

            [Display(Name = "Postal Code")]
            public string PostalZip { get; set; }

            [Display(Name = "Country")]
            public string Country { get; set; }

            [Display(Name = "Instructions")]
            public string SpecialInstructions { get; set; }

            [Display(Name = "Commercial Property")]
            public bool Commercial { get; set; }

            [Display(Name = "Tracking Number")]
            public string TrackingNumber { get; set; }
        }

        [NotMapped]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; }
        [NotMapped]
        public List<UserSelection> UserSelections { get; set; }
        [NotMapped]
        public List<Product> Products { get; set; }
        [NotMapped]
        public string Product { get; set; }
        [NotMapped]
        public string Notes { get; set; }
        [NotMapped]
        public SelectList Languages { get; set; }
    }

    [ModelMetadataType(typeof(UserSelectionMetadata))]
    public partial class UserSelection
    {
        public class UserSelectionMetadata
        {
        }
        [NotMapped]
        public byte[] Image { get; set; }
        [NotMapped]
        public string Name { get; set; }
        [NotMapped]
        public List<ProductOption> Options { get; set; }
    }

    [ModelMetadataType(typeof(ProductMetadata))]
    public partial class Product
    {
        public class ProductMetadata
        {
            [Display(Name = "Dealer Code")]
            [Required]
            public string DealerCode { get; set; }

            [Display(Name = "Purchase Order")]
            public string Ponumber { get; set; }

            [Display(Name = "Chair Name")]
            public string Chair { get; set; }

            [Display(Name = "Chair Image")]
            public string Image { get; set; }

            [Display(Name = "Language")]
            public string Language { get; set; }

            [Display(Name = "Company Logo File")]
            public string LogoFile { get; set; }

            [Display(Name = "Dealer Logo File")]
            public string LogoFile2 { get; set; }

            [Display(Name = "Install Guide")]
            public string InstallGuide { get; set; }

            [Display(Name = "User Guide")]
            public string UserGuide { get; set; }

            [Display(Name = "Video Url")]
            public string VideoUrl { get; set; }

            [Display(Name = "Sit Fit Guide")]
            public string SitFitGuide { get; set; }

            [Display(Name = "Verify Only")]
            public bool VerifyOnly { get; set; }

            [Display(Name = "Shipper")]
            public string Shipper { get; set; }
        }

        [Display(Name = "Upload Company Logo File")]
        [NotMapped]
        public IFormFile FormFile { get; set; }

        [Display(Name = "Upload Dealer Logo File")]
        [NotMapped]
        public IFormFile FormFile2 { get; set; }

        [Display(Name = "Upload Chair Image File")]
        [NotMapped]
        public IFormFile FormFile3 { get; set; }

        [Display(Name = "Tracking Number")]
        [NotMapped]
        public string TrackingNumber { get; set; }

        [NotMapped]
        public SelectList Languages { get; set; }

        [NotMapped]
        public SelectList Shippers { get; set; }
    }

    [ModelMetadataType(typeof(ProductOptionMetadata))]
    public partial class ProductOption
    {
        public class ProductOptionMetadata
        {
            [Display(Name = "Option Type")]
            [Required]
            public string Type { get; set; }

            [Display(Name = "Option Name")]
            [Required]
            public string Name { get; set; }

            [Display(Name = "Image File")]
            public string FileName { get; set; }
        }

        [Display(Name = "Upload Image File")]
        [NotMapped]
        public IFormFile FormFile { get; set; }
        [NotMapped]
        public string Product { get; set; }
        [NotMapped]
        public SelectList Types { get; set; }
    }

    [ModelMetadataType(typeof(ProductImageMetadata))]
    public partial class ProductImage
    {
        public class ProductImageMetadata
        {
            [Display(Name = "Fabric")]
            [Required]
            public int ProductOption1Id { get; set; }

            [Display(Name = "Mesh")]
            [Required]
            public int ProductOption2Id { get; set; }

            [Display(Name = "Frame")]
            [Required]
            public int ProductOption3Id { get; set; }

            [Display(Name = "Image File")]
            public string FileName { get; set; }
        }

        [Display(Name = "Upload Image File")]
        [NotMapped]
        public IFormFile FormFile { get; set; }
        [NotMapped]
        public string Product { get; set; }
        [NotMapped]
        public string Option1 { get; set; }
        [NotMapped]
        public string Option2 { get; set; }
        [NotMapped]
        public string Option3 { get; set; }
        [NotMapped]
        public SelectList Options1 { get; set; }
        [NotMapped]
        public SelectList Options2 { get; set; }
        [NotMapped]
        public SelectList Options3 { get; set; }
    }
}