Imports System.Net
Public Class Response

    Private _response As HttpWebResponse
    Friend Sub New(_nativeResponse As HttpWebResponse)
        _response = _nativeResponse
    End Sub

    Public Function GetAsString() As String
        If _response Is Nothing Then Return ""

        Dim contents As String
        Using sr = New IO.StreamReader(_response.GetResponseStream())
            contents = sr.ReadToEnd()
        End Using
        Return contents
    End Function

    Public Function GetAsBytes() As Byte()
        If _response Is Nothing Then Return Nothing
        Return Text.ASCIIEncoding.ASCII.GetBytes(GetAsString)
    End Function

    Public Function GetAsHTTPResponse() As HttpWebResponse
        Return _response
    End Function

End Class