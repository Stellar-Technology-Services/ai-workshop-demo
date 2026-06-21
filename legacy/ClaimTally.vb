' Legacy claims tally. Kept OUT of the build on purpose (this folder is not part
' of any .csproj). It exists as a workshop target for /explain-legacy and for a
' translate-to-C# exercise.
'
' It computes the same tally the C# ClaimsDashboard produces over the real
' database: how many claims there are, and how many are missing a claim number.

Option Explicit On

Module ClaimTally

    ' A claim number that is Nothing, blank, or still a bracketed placeholder
    ' (e.g. "[7-digit claim number]") is counted as "missing".
    Public Function BuildTally(ByVal claimNumbers() As String) As String
        Dim total As Integer
        Dim missing As Integer
        Dim i As Integer

        total = 0
        missing = 0

        For i = 0 To UBound(claimNumbers)
            total = total + 1

            Dim c As String
            c = claimNumbers(i)

            If c Is Nothing Then
                missing = missing + 1
            ElseIf Trim(c) = "" Then
                missing = missing + 1
            ElseIf Left(Trim(c), 1) = "[" And Right(Trim(c), 1) = "]" Then
                missing = missing + 1
            End If
        Next

        Dim withNumber As Integer
        withNumber = total - missing

        Dim report As String
        report = "Claims tally" & vbCrLf
        report = report & "------------" & vbCrLf
        report = report & "Total claims:         " & CStr(total) & vbCrLf
        report = report & "With claim number:    " & CStr(withNumber) & vbCrLf
        report = report & "Missing claim number: " & CStr(missing) & vbCrLf

        Return report
    End Function

    Sub Main()
        Dim sample() As String = { _
            "CLM-1001", _
            "CLM-1002", _
            "[7-digit claim number]", _
            "", _
            "CLM-1005" _
        }

        Console.WriteLine(BuildTally(sample))
    End Sub

End Module
