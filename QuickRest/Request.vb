''Copyright 2017 Kennedy Tochukwu Ekeoha

''Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
''And associated documentation files (the "Software"), to deal in the Software without restriction,
''including without limitation the rights To use, copy, modify, merge, publish, distribute, sublicense,
''And/Or sell copies Of the Software, And To permit persons To whom the Software Is furnished 
''To Do so, subject To the following conditions:

''The above copyright notice and this permission notice shall be included in all copies or 
''substantial portions Of the Software.

''THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING 
''BUT Not LIMITED To THE WARRANTIES Of MERCHANTABILITY, FITNESS For A PARTICULAR PURPOSE And 
''NONINFRINGEMENT.In NO Event SHALL THE AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, 
''DAMAGES Or OTHER LIABILITY, WHETHER In AN ACTION Of CONTRACT, TORT Or OTHERWISE, ARISING FROM, 
''OUT OF Or IN CONNECTION WITH THE SOFTWARE Or THE USE Or OTHER DEALINGS IN THE SOFTWARE.


Imports System.ComponentModel
Imports System.Net

Public Class Request

    Dim sb As Text.StringBuilder = New Text.StringBuilder
    'Dim WithEvents worker As BackgroundWorker = New BackgroundWorker
    Dim queryParams As Dictionary(Of String, String) = New Dictionary(Of String, String)
    Dim urlparams As Dictionary(Of String, String) = New Dictionary(Of String, String)
    Dim headers As Dictionary(Of String, String) = New Dictionary(Of String, String)
    Dim fragment As String = ""
    Dim method As String = "GET"
    Dim url As String = ""
    Dim content As String = ""
    Private wrequest As HttpWebRequest

    Private _isInFlight As Boolean
    Public Property IsInFlight() As Boolean
        Get
            Return _isInFlight
        End Get
        Set(ByVal value As Boolean)
            _isInFlight = value
        End Set
    End Property

    ''' <summary>
    ''' Creates a new Request.
    ''' </summary>
    ''' <param name="pURL">the url. don't add a fragment and a query string.</param>
    ''' <param name="pMethod">the method to use.</param>
    Public Sub New(ByVal pURL As String, Optional pMethod As String = "GET")
        url = pURL
        method = pMethod
        _isInFlight = False
    End Sub

    ''' <summary>
    ''' Replace any query param 
    ''' </summary>
    ''' <param name="name">the NAME of the query parameter</param>
    ''' <param name="value">the VALUE of the query parameter.</param>
    ''' <returns>The Request Object. useful for chaining.</returns>
    ''' <remarks>if the url param has already been the declared, it will replace the old value with the new one.</remarks>
    Public Function SetQueryParam(ByVal name As String, ByVal value As String) As Request
        name = name.ToLower
        If queryParams.ContainsKey(name) Then
            queryParams(name) = value
        Else
            queryParams.Add(name, value)
        End If
        Return Me
    End Function

    ''' <summary>
    ''' Replace any url param ( in the {url param} form. e.g. {target})
    ''' </summary>
    ''' <param name="name">url param without the curly brackets. e.g. want to add the {target} param? pass in just "target"</param>
    ''' <param name="value">the value that will replace the url param in the url.</param>
    ''' <returns>The Request Object. useful for chaining.</returns>
    ''' <remarks>if the url param has already been the declared, it will replace the old value with the new one.</remarks>
    Public Function SetUrlParam(ByVal name As String, ByVal value As String) As Request
        name = name.ToLower
        If urlparams.ContainsKey(name) Then
            urlparams(name) = value
        Else
            urlparams.Add(name, value)
        End If
        Return Me
    End Function

    ''' <summary>
    ''' Set headers.
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="value"></param>
    ''' <remarks>if the header has already been the declared, it will replace the old value with the new one.</remarks>
    Public Function SetHeader(ByVal name As String, ByVal value As String) As Request
        name = name.ToLower
        If headers.ContainsKey(name) Then
            headers(name) = value
        Else
            headers.Add(name, value)
        End If
        Return Me
    End Function

    ''' <summary>
    ''' Set the fragment. the part after the "#" symbol in standard URLs.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns>The request Object.</returns>
    ''' <remarks>if the fragment has already been the declared, it will replace the old value with the new one.</remarks>
    Public Function SetFragment(ByVal value As String) As Request
        fragment = value
        Return Me
    End Function

    ''' <summary>
    ''' Set the method for this request. the default value for a Request is GET. use this to change it.
    ''' </summary>
    ''' <param name="value">the method to be used. if value is not valid, request will be reset to GET.</param>
    ''' <returns></returns>
    Public Function SetMethod(ByVal value As String) As Request
        If String.IsNullOrEmpty(method) Then
            method = "GET"
        Else
            method = value.ToUpper
        End If
        'TODO check method with the standard allowed ones.
        Return Me
    End Function

    ''' <summary>
    ''' Send the Request. Asynchronous.
    ''' </summary>
    ''' <param name="timeout"></param>
    ''' <returns></returns>
    Public Function Send(Optional timeout As Long = -1)
        'can't send\resend if the worker is busy...
        'If worker.IsBusy Then Return False


        Try

            _isInFlight = True
            'configuration data
            Dim conf As RequestConfiguration = New RequestConfiguration() With {.Timeout = timeout}

            'host
            sb.Append(url)

            'set url params - replace them in the url
            For Each urlparam In urlparams
                Dim _val = "{" & urlparam.Key & "}"
                sb.Replace(_val, urlparam.Value)
            Next

            'add query string
            If queryParams.Count > 0 Then sb.Append("?")
            Dim queryParamCurIndex As Integer = 0
            Dim queryparamscount As Integer = queryParams.Count - 1
            For Each queryparam In queryParams

                sb.Append(String.Format("{0}={1}", queryparam.Key, queryparam.Value))

                'don't append ampersand to the last query param
                If queryParamCurIndex < queryparamscount Then
                    sb.Append("&")
                End If

                queryParamCurIndex += 1
            Next


            'append fragment
            If Not String.IsNullOrEmpty(fragment) Then sb.Append("#" & fragment)



            'build actual WebRequest -- uri
            wrequest = HttpWebRequest.Create(sb.ToString)

            'build actual WebRequest -- method
            wrequest.Method = method

            'webrequest wants to set content-length and type directly in the object and not with other headers.
            If headers.ContainsKey("content-length") Then
                wrequest.ContentLength = Long.Parse(headers("content-length")) 'TODO: check value before assignment
            End If

            If headers.ContainsKey("content-type") Then
                wrequest.ContentType = Long.Parse(headers("content-type")) 'TODO: check value before assignment
            End If

            'build actual WebRequest -- add headers
            For Each pair In headers
                If pair.Key = "content-length" Or pair.Key = "content-type" Then
                    'already added :)
                Else
                    wrequest.Headers.Add(pair.Key, pair.Value)
                End If
            Next

            'build actual WebRequest -- add content
            If (Not String.IsNullOrEmpty(content)) Then
                Dim _data = Text.Encoding.UTF8.GetBytes(content)
                wrequest.GetRequestStream.Write(_data, 0, _data.Length)
            End If

            If (conf.Timeout >= 0) Then
                wrequest.Timeout = conf.Timeout
            End If


            'SEND!!
            wrequest.BeginGetResponse(AddressOf ResponseCallbackInternal, wrequest)

            'Pass the result to the handlers.
            ' e.Result = New Response(nativeHTTPResponse)
        Catch ex As Exception
            Debug.Print(ex.Message)
            _isInFlight = False
        Finally

        End Try

        Return True
    End Function

    Private Sub ResponseCallbackInternal(ar As IAsyncResult)
        If ar.IsCompleted Then
            Dim _curRequest As HttpWebRequest = ar.AsyncState
            RaiseEvent OnResponse(Me,
                                  New Response(_curRequest.EndGetResponse(ar)))
            IsInFlight = False
        End If
    End Sub

    ''' <summary>
    ''' Add the content of the message. used by PUT, UPDATE, POST requests.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    Public Function SetContent(ByVal value As String)
        content = value
        Return Me
    End Function

    Public Function ClearQueryParams() As Request
        If _isInFlight Then Return Me
        queryParams.Clear()
        Return Me
    End Function

    Public Function ClearUrlParams() As Request
        If _isInFlight Then Return Me
        urlparams.Clear()
        Return Me
    End Function

    Public Function ClearHeaders() As Request
        If _isInFlight Then Return Me
        headers.Clear()
        Return Me
    End Function
    ''' <summary>
    ''' Reset the Request to the default values and removes all parameters and headers.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>Doesn't work if it is in flight.</remarks>
    Public Function Reset() As Request
        If _isInFlight Then Return Me
        ClearQueryParams()
        ClearUrlParams()
        ClearHeaders()

        content = ""
        fragment = ""
        method = "GET"
        url = ""
        wrequest = Nothing
        Return Me
    End Function

    Public Function Cancel() As Request
        wrequest.Abort()

        Return Me
    End Function


    Public Event OnResponse(ByVal sender As Request, ByVal response As Response)



End Class

'Public Class OldRequest

'    Dim sb As Text.StringBuilder = New Text.StringBuilder
'    Dim WithEvents worker As BackgroundWorker = New BackgroundWorker
'    Dim queryParams As Dictionary(Of String, String) = New Dictionary(Of String, String)
'    Dim urlparams As Dictionary(Of String, String) = New Dictionary(Of String, String)
'    Dim headers As Dictionary(Of String, String) = New Dictionary(Of String, String)
'    Dim fragment As String = ""
'    Dim method As String = "GET"
'    Dim url As String = ""
'    Dim content As String = ""
'    Private _canSend As Boolean
'    Public Property CanSend() As Boolean
'        Get
'            Return _canSend
'        End Get
'        Set(ByVal value As Boolean)
'            _canSend = value
'        End Set
'    End Property

'    ''' <summary>
'    ''' Creates a new Request.
'    ''' </summary>
'    ''' <param name="pURL">the url. don't add a fragment and a query string.</param>
'    ''' <param name="pMethod">the method to use.</param>
'    Public Sub New(ByVal pURL As String, Optional pMethod As String = "GET")
'        url = pURL
'        method = pMethod
'        _canSend = True
'    End Sub

'    ''' <summary>
'    ''' Replace any query param 
'    ''' </summary>
'    ''' <param name="name">the NAME of the query parameter</param>
'    ''' <param name="value">the VALUE of the query parameter.</param>
'    ''' <returns>The Request Object. useful for chaining.</returns>
'    ''' <remarks>if the url param has already been the declared, it will replace the old value with the new one.</remarks>
'    Public Function SetQueryParam(ByVal name As String, ByVal value As String) As Request
'        name = name.ToLower
'        If queryParams.ContainsKey(name) Then
'            queryParams(name) = value
'        Else
'            queryParams.Add(name, value)
'        End If
'        Return Me
'    End Function

'    ''' <summary>
'    ''' Replace any url param ( in the {url param} form. e.g. {target})
'    ''' </summary>
'    ''' <param name="name">url param without the curly brackets. e.g. want to add the {target} param? pass in just "target"</param>
'    ''' <param name="value">the value that will replace the url param in the url.</param>
'    ''' <returns>The Request Object. useful for chaining.</returns>
'    ''' <remarks>if the url param has already been the declared, it will replace the old value with the new one.</remarks>
'    Public Function SetUrlParam(ByVal name As String, ByVal value As String) As Request
'        name = name.ToLower
'        If urlparams.ContainsKey(name) Then
'            urlparams(name) = value
'        Else
'            urlparams.Add(name, value)
'        End If
'        Return Me
'    End Function

'    ''' <summary>
'    ''' Set headers.
'    ''' </summary>
'    ''' <param name="name"></param>
'    ''' <param name="value"></param>
'    ''' <remarks>if the header has already been the declared, it will replace the old value with the new one.</remarks>
'    Public Function SetHeader(ByVal name As String, ByVal value As String) As Request
'        name = name.ToLower
'        If headers.ContainsKey(name) Then
'            headers(name) = value
'        Else
'            headers.Add(name, value)
'        End If
'        Return Me
'    End Function

'    ''' <summary>
'    ''' Set the fragment. the part after the "#" symbol in standard URLs.
'    ''' </summary>
'    ''' <param name="value"></param>
'    ''' <returns>The request Object.</returns>
'    ''' <remarks>if the fragment has already been the declared, it will replace the old value with the new one.</remarks>
'    Public Function SetFragment(ByVal value As String) As Request
'        fragment = value
'        Return Me
'    End Function

'    ''' <summary>
'    ''' Set the method for this request. the default value for a Request is GET. use this to change it.
'    ''' </summary>
'    ''' <param name="value">the method to be used. if value is not valid, request will be reset to GET.</param>
'    ''' <returns></returns>
'    Public Function SetMethod(ByVal value As String) As Request
'        If String.IsNullOrEmpty(method) Then
'            method = "GET"
'        Else
'            method = value.ToUpper
'        End If
'        'TODO check method with the standard allowed ones.
'        Return Me
'    End Function

'    ''' <summary>
'    ''' Send the Request. Asynchronous.
'    ''' </summary>
'    ''' <param name="timeout"></param>
'    ''' <returns></returns>
'    Public Function Send(Optional timeout As Long = -1)
'        'can't send\resend if the worker is busy...
'        If worker.IsBusy Then Return False


'        Try
'            Dim configuration = New RequestConfiguration() With {.Timeout = timeout}
'            _canSend = False
'            worker.RunWorkerAsync(configuration)
'        Catch ex As Exception
'            Debug.Print(ex.Message)
'            _canSend = True
'        Finally

'        End Try

'        Return True
'    End Function

'    ''' <summary>
'    ''' Add the content of the message. used by PUT, UPDATE, POST requests.
'    ''' </summary>
'    ''' <param name="value"></param>
'    ''' <returns></returns>
'    Public Function SetContent(ByVal value As String)
'        content = value
'        Return Me
'    End Function

'    Public Function ClearQueryParams() As Request
'        queryParams.Clear()
'        Return Me
'    End Function

'    Public Function ClearUrlParams() As Request
'        urlparams.Clear()
'        Return Me
'    End Function

'    Public Function ClearHeaders() As Request
'        headers.Clear()
'        Return Me
'    End Function

'    Public Function Reset() As Request
'        ClearQueryParams()
'        ClearUrlParams()
'        ClearHeaders()

'        content = ""
'        fragment = ""
'        method = "GET"
'        url = ""
'        Return Me
'    End Function

'    Public Function Cancel() As Request
'        Throw New NotImplementedException
'    End Function


'    Private Sub worker_DoWork(sender As Object, e As DoWorkEventArgs) Handles worker.DoWork
'        'configuration data
'        Dim conf As RequestConfiguration = e.Argument

'        'host
'        sb.Append(url)

'        'set url params - replace them in the url
'        For Each urlparam In urlparams
'            Dim _val = "{" & urlparam.Key & "}"
'            sb.Replace(_val, urlparam.Value)
'        Next

'        'add query string
'        If queryParams.Count > 0 Then sb.Append("?")
'        Dim queryParamCurIndex As Integer = 0
'        Dim queryparamscount As Integer = queryParams.Count - 1
'        For Each queryparam In queryParams

'            sb.Append(String.Format("{0}={1}", queryparam.Key, queryparam.Value))

'            'don't append ampersand to the last query param
'            If queryParamCurIndex < queryparamscount Then
'                sb.Append("&")
'            End If

'            queryParamCurIndex += 1
'        Next


'        'append fragment
'        If Not String.IsNullOrEmpty(fragment) Then sb.Append("#" & fragment)



'        'build actual WebRequest -- uri
'        Dim wrequest = HttpWebRequest.Create(sb.ToString)

'        'build actual WebRequest -- method
'        wrequest.Method = method

'        'webrequest wants to set content-length and type directly in the object and not with other headers.
'        If headers.ContainsKey("content-length") Then
'            wrequest.ContentLength = Long.Parse(headers("content-length")) 'TODO: check value before assignment
'        End If

'        If headers.ContainsKey("content-type") Then
'            wrequest.ContentType = Long.Parse(headers("content-type")) 'TODO: check value before assignment
'        End If

'        'build actual WebRequest -- add headers
'        For Each pair In headers
'            If pair.Key = "content-length" Or pair.Key = "content-type" Then
'                'already added :)
'            Else
'                wrequest.Headers.Add(pair.Key, pair.Value)
'            End If
'        Next

'        'build actual WebRequest -- add content
'        If (Not String.IsNullOrEmpty(content)) Then
'            Dim _data = Text.Encoding.UTF8.GetBytes(content)
'            wrequest.GetRequestStream.Write(_data, 0, _data.Length)
'        End If

'        If (conf.Timeout >= 0) Then
'            wrequest.Timeout = conf.Timeout
'        End If


'        'SEND!!
'        Dim nativeHTTPResponse = wrequest.GetResponse


'        'Pass the result to the handlers.
'        e.Result = New Response(nativeHTTPResponse)
'    End Sub

'    Private Sub worker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles worker.RunWorkerCompleted
'        _canSend = True
'        RaiseEvent OnResponse(Me, DirectCast(e.Result, Response))
'    End Sub






'    Public Event OnResponse(ByVal sender As Request, ByVal response As Response)



'End Class

''' <summary>
''' basic configuration for the request.
''' </summary>
Public Structure RequestConfiguration
    Public Timeout As Long
End Structure