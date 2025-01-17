using System.Text;
using LLama.Common;
using static LLama.Common.ChatHistory;
const string StartHeaderId = "<|im_start|>";
const string EndHeaderId = "<|im_end|>\n";
void EncodeHeader(Message message, StringBuilder sb)
{
    sb.Append(StartHeaderId);
    sb.Append(message.AuthorRole.ToString().ToLower());
    sb.Append('\n');
}
void EncodeMessage(Message message, StringBuilder sb)
{
    EncodeHeader(message, sb);
    sb.Append(message.Content);
    sb.Append(EndHeaderId);
}

StringBuilder StrBuilder = new StringBuilder();
foreach (var message in Messages)
    EncodeMessage(message, StrBuilder);
EncodeHeader(new Message(AuthorRole.Assistant, ""), StrBuilder);
return StrBuilder.ToString();