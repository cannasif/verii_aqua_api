using System.ComponentModel.DataAnnotations;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class AquaSettingsDto
    {
        public int PartialTransferOccupiedCageMode { get; set; }
    }

    public class UpdateAquaSettingsDto
    {
        [Range(0, 2)]
        public int PartialTransferOccupiedCageMode { get; set; } = 0;
    }
}
