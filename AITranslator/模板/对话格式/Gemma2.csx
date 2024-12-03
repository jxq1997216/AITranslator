using System.Text;
using LLama.Common;
using static LLama.Common.ChatHistory;
const string StartHeaderId = "<start_of_turn>";
const string EndHeaderId = "<end_of_turn>\n";
void EncodeHeader(Message message, StringBuilder sb)
{
    sb.Append(StartHeaderId);
    sb.Append(message.AuthorRole.ToString());
    sb.Append('\n');
}
void EncodeMessage(Message message, StringBuilder sb)
{
    EncodeHeader(message, sb);
    sb.Append(message.Content);
    sb.Append(EndHeaderId);
}

StringBuilder StrBuilder = new StringBuilder();
StrBuilder.Append("<bos>\n");
foreach (var message in Messages)
    EncodeMessage(message, StrBuilder);
EncodeHeader(new Message(AuthorRole.Assistant, ""), StrBuilder);
return StrBuilder.ToString();