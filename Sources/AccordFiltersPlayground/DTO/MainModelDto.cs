using System.Collections.Generic;

namespace AccordFiltersPlayground.DTO
{
    public class MainModelDto
    {
        public List<CodedFilterDto> CodedFilters { get; set; }

        public List<string> InputFilePaths { get; set; }
    }
}