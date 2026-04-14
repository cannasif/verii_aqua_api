using System;
using System.ComponentModel.DataAnnotations;

namespace aqua_api.Modules.Identity.Application.Dtos
{
    public class UserDetailDto : BaseEntityDto
    {
        public long UserId { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? Description { get; set; }
        public Gender? Gender { get; set; }
    }

    public class CreateUserDetailDto
    {
        [Required]
        public long UserId { get; set; }

        [StringLength(500)]
        public string? ProfilePictureUrl { get; set; }

        [Range(0, 300)]
        public decimal? Height { get; set; }

        [Range(0, 500)]
        public decimal? Weight { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public Gender? Gender { get; set; }
    }

    public class UpdateUserDetailDto
    {
        [StringLength(500)]
        public string? ProfilePictureUrl { get; set; }

        [Range(0, 300)]
        public decimal? Height { get; set; }

        [Range(0, 500)]
        public decimal? Weight { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public Gender? Gender { get; set; }
    }
}
