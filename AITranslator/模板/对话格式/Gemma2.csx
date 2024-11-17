using System.Text;
using LLama.Common;
using static LLama.Common.ChatHistory;

private void EncodeHeader(Message message, StringBuilder sb)
{
    sb.Append(StartHeaderId);
    sb.Append(message.AuthorRole.ToString());
    sb.Append('\n');
}

private void EncodeMessage(Message message, StringBuilder sb)
{
    EncodeHeader(message, sb);
    sb.Append(message.Content);
    sb.Append(EndHeaderId);
}

const string StartHeaderId = "<|im_start|>";
const string EndHeaderId = "<|im_end|>\n";

StringBuilder StrBuilder = new StringBuilder();
foreach (var message in Messages)
    EncodeMessage(message, StrBuilder);
EncodeHeader(new Message(AuthorRole.Assistant, ""), StrBuilder);

return StrBuilder.ToString();