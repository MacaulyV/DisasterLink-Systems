using System.Collections.Generic;

namespace DisasterLink_API.DTOs.Common
{
    public abstract class LinkedResourceDto
    {
        public List<ResourceLink> Links { get; set; } = new List<ResourceLink>();
    }
} 