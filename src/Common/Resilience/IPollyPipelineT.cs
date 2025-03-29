using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace MarcRocNy.Common.Resilience;

public interface IPollyPipeline<TMarker>
{
    ResiliencePipeline Pipeline { get; }
}
