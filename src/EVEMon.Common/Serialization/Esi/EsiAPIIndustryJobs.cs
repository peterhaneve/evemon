﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EVEMon.Common.Serialization.Esi
{
    [CollectionDataContract]
    public sealed class EsiAPIIndustryJobs : List<EsiJobListItem>
    {
    }
}
