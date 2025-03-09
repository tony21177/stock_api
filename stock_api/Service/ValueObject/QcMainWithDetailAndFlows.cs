using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class QcMainWithDetailAndFlows : QcMainWithDetail
    {

        public List<QcFlowWithAgentsVo>? Flows { get; set; }
        public List<QcFlowLog>? FlowLogs { get; set; }

    }


}
