using LLama.Common;
using LLama.Sampling;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.PostData
{
    public class PostDataNormal
    {
        public PostDataBase Base { get; set; }
        public string Expended { get; set; }

        public override string ToString()
        {
            string sendJson = JsonConvert.SerializeObject(Base);
            if (!string.IsNullOrWhiteSpace(Expended))
                sendJson = sendJson.Insert(sendJson.Length - 1, "," + Expended);
            return sendJson;
        }

        public InferenceParams ToInferenceParams()
        {
            return new InferenceParams
            {
                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = (float)Base.temperature,
                    FrequencyPenalty = (float)Base.frequency_penalty,
                    PresencePenalty = (float)Base.presence_penalty,
                    TopP = (float)Base.top_p,
                },

                AntiPrompts = Base.stop,
                MaxTokens = Base.max_tokens,
            };
        }
    }

}
