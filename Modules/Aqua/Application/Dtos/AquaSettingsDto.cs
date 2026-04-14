using System.ComponentModel.DataAnnotations;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class AquaSettingsDto
    {
        public bool RequireFullTransfer { get; set; }
        public bool AllowProjectMerge { get; set; }
        public int PartialTransferOccupiedCageMode { get; set; }
    }

    public class UpdateAquaSettingsDto
    {
        public bool RequireFullTransfer { get; set; } = true;
        public bool AllowProjectMerge { get; set; } = false;

        [Range(0, 2)]
        public int PartialTransferOccupiedCageMode { get; set; } = 0;
    }
}
