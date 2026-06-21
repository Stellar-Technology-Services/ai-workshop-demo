using System.Text;
using ClaimsChat.Data;

namespace ClaimsChat.Services.Stats;

// Builds a plain-text summary of the seeded claim documents: how many claims we
// have, how many are missing a claim number, and how large the documents are.
// App-layer reporting, distinct from the sealed-box AI client: the chat can
// answer questions about a single document, but it cannot tally the whole
// collection the way this does.
//
// NOTE: this has grown over time and could use a cleanup pass.
public class ClaimsDashboard
{
    public string Render(IEnumerable<Document>? documents)
    {
        var list = new List<Document>();
        if (documents != null)
        {
            foreach (var d in documents)
            {
                list.Add(d);
            }
        }

        // total
        int n = 0;
        for (int x = 0; x < list.Count; x++)
        {
            n = n + 1;
        }

        // how many are missing a claim number
        int missing = 0;
        foreach (var d in list)
        {
            var cn = d.ClaimNumber;
            if (cn == null)
            {
                missing++;
            }
            else if (cn.Trim() == "")
            {
                missing++;
            }
            else if (cn.Trim().StartsWith("[") && cn.Trim().EndsWith("]"))
            {
                // some old rows still have the placeholder text in them
                missing++;
            }
        }

        int withNumber = n - missing;

        // body sizes (characters)
        int totalLen = 0;
        int min = 0;
        int max = 0;
        bool first = true;
        foreach (var d in list)
        {
            int len = 0;
            if (d.Body != null)
            {
                len = d.Body.Length;
            }

            totalLen = totalLen + len;

            if (first)
            {
                min = len;
                max = len;
                first = false;
            }
            else
            {
                if (len < min) min = len;
                if (len > max) max = len;
            }
        }

        int avg = 0;
        if (n > 0)
        {
            avg = totalLen / n;
        }

        // percentage with a claim number
        int pct = 0;
        if (n != 0)
        {
            pct = (int)((withNumber * 100.0) / n);
        }

        // most recent seed time
        DateTime latest = DateTime.MinValue;
        foreach (var d in list)
        {
            if (d.SeededAtUtc > latest)
            {
                latest = d.SeededAtUtc;
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("Claims dashboard");
        sb.AppendLine("================");
        sb.AppendLine("Total claims:          " + n);
        sb.AppendLine("With claim number:     " + withNumber + " (" + pct + "%)");
        sb.AppendLine("Missing claim number:  " + missing);
        if (n > 0)
        {
            sb.AppendLine("Body length (chars):   avg " + avg + ", min " + min + ", max " + max);
            sb.AppendLine("Last seeded (UTC):     " + latest.ToString("yyyy-MM-dd HH:mm"));
        }
        else
        {
            sb.AppendLine("No claims found.");
        }

        return sb.ToString();
    }
}
