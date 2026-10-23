
using System.ComponentModel.DataAnnotations;

namespace SorteFederation.GlobalEnums;

public enum MimeTypeEnum
{
    [Display(Name = "text/plain")]
    Text = 1,

    [Display(Name = "application/pdf")]
    Pdf,

    [Display(Name = "application/vnd.ms-word")]
    Doc,

    [Display(Name = "application/vnd.ms-word")]
    Docx,

    [Display(Name = "application/vnd.ms-excel")]
    Xls,

    [Display(Name = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    Xlsx,

    [Display(Name = "image/png")]
    Png,

    [Display(Name = "image/jpeg")]
    Jpg,

    [Display(Name = "image/jpeg")]
    Jpeg,

    [Display(Name = "image/gif")]
    Gif,

    [Display(Name = "text/csv")]
    Cvs,

}
