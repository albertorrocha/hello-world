Imports System.IO
Imports System.IO.Compression
Imports System.IO.Packaging
Imports System.Net.Mail
Imports wfUploadFiles.ERPDD.DO.Service

Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Timer1 = New Timer
        Timer1.Enabled = True
        Timer1.Interval = 3600000

        Timer2 = New Timer
        Timer2.Enabled = True
        Timer2.Interval = 1000

        Timer3 = New Timer
        Timer3.Enabled = True
        Timer3.Interval = 90000

        Label1.Text = Format(Date.Now, "HH:mm:ss")
        Label2.Text = ""
    End Sub
    Private Sub Starting(sender As Object, e As EventArgs)
        Label2.Text = "Última Atualização " + Format(Date.Now, "dd/MM/yyyy HH:mm").ToString()
    End Sub
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim data As DateTime
        '------------------------
        data = Date.Now
        Label1.Text = Format(Date.Now, "HH:mm:ss")
        ProgressBar1.Minimum = 0
        ProgressBar1.Maximum = 0
        If (data.Hour) = 2 Then
            Starting(sender, e)
            'If (data.DayOfWeek) = 5 Then
            CopyFiles()
            'End If
        End If
    End Sub
    Private Function CompactFile(ByRef Msg As String, ByVal Caminho As String) As Boolean
        Try
            ZipFile.CreateFromDirectory(Caminho, Caminho & ".zip")
            Return True
        Catch ex As Exception
            Msg = ex.Message
            Return False
        End Try
    End Function
    Private Function UploadFiles(ByRef Msg As String, ByVal Caminho As String, ByVal NomeArquivo As String) As Boolean
        Try
            Dim Request As System.Net.FtpWebRequest
            Dim Strz As System.IO.Stream
            Dim File() As Byte

            Dim FTP As String
            Dim User As String
            Dim Pass As String
            '----------------------------------------
            FTP = ""
            User = ""
            Pass = ""
            '
            FTP += "PASTA/" + NomeArquivo.ToString()
            Request = DirectCast(System.Net.WebRequest.Create(FTP), System.Net.FtpWebRequest)
            Request.Credentials = New System.Net.NetworkCredential(User, Pass)
            Request.Method = System.Net.WebRequestMethods.Ftp.UploadFile

            File = System.IO.File.ReadAllBytes(Caminho + NomeArquivo)

            Strz = Request.GetRequestStream()
            Strz.Write(File, 0, File.Length)
            Strz.Close()
            Strz.Dispose()
            Msg = "Upload Completed"
            Return True
        Catch ex As Exception
            Msg = ex.Message.ToString() & vbCrLf
            Msg += "Data: " + Format(Date.Now, "dd/MM/yy HH:mm")
            If Not System.Diagnostics.Debugger.IsAttached Then
                EnviaEmailSMTP(Msg, "teste@teste.com.br", "teste@teste.com.br", "", "Upload - " + ex.Message.ToString(), 1, 1, Nothing)
            End If
            Return False
        End Try
    End Function
    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        Label1.Text = Format(Date.Now, "HH:mm:ss")
    End Sub
    Private Sub CopyFiles()
        Try
            Dim Destino As String
            Dim Caminho As String
            Dim Arquivos() As String
            Dim NomeArquivo As String
            '-----------------------
            Caminho = "PASTA\"
            Destino = Caminho & Format(Date.Now, "yyyyMMdd")
            Directory.CreateDirectory(Destino)
            Arquivos = Directory.GetFiles(Caminho, "*")
            '
            For i As Integer = 0 To UBound(Arquivos)
                '
                NomeArquivo = Arquivos(i).Replace(Caminho, "").Trim()
                System.IO.File.Copy(Arquivos(i), Destino & "\" & NomeArquivo)
                System.IO.File.Delete(Arquivos(i))
            Next
            '
            Arquivos = Directory.GetDirectories(Caminho)
            If Arquivos.Length >= 8 Then
                For i As Integer = 0 To 0
                    System.IO.Directory.Delete(Arquivos(i), True)
                Next
            End If
            '
        Catch ex As Exception

        End Try
    End Sub

    Private Sub CopyFolder(ByVal Caminho As String, ByVal Destino As String, ByVal Extensao As String)
        Dim Arquivos() As String
        Dim NomeArquivo As String
        '---------------------------

        Arquivos = Directory.GetFiles(Caminho, "*." & Extensao & "")
        Directory.CreateDirectory(Destino)
        '
        For i As Integer = 0 To UBound(Arquivos)
            '
            NomeArquivo = Arquivos(i).Replace(Caminho, "").Trim()
            System.IO.File.Copy(Arquivos(i), Destino & "\" & NomeArquivo)
            System.IO.File.Delete(Arquivos(i))
        Next
    End Sub
    Private Function EnviaEmailSMTP(ByVal Mensagem As String,
                          ByVal Remetente As String,
                          ByVal Destinatario As String,
                          ByVal CopiaOculta As String,
                          ByVal Titulo As String,
                          ByVal id_filial As String,
                          ByVal id_pedido As String,
                          ByVal Arquivos As List(Of String)) As Boolean
        Try
            EnviaEmailSMTP = True
            'Cria uma instância da classe MailMessage
            Dim mail As New MailMessage()
            Dim vDest() As String = Split(Destinatario, ";")
            For i As Integer = 0 To UBound(vDest)
                If Len(vDest(i)) > 0 Then mail.[To].Add(vDest(i)) 'destino
            Next
            If Len(CopiaOculta) > 0 Then mail.[Bcc].Add(CopiaOculta) 'Cópia Oculta
            mail.From = New MailAddress(Remetente, "REMETENTE") 'Remetente
            mail.Subject = Titulo ''assunto
            mail.Body = Mensagem '  'corpo do email (texto)
            mail.IsBodyHtml = True ''é html ?
            Try
                For i As Integer = 0 To Arquivos.Count - 1
                    mail.Attachments.Add(New Attachment(Arquivos(i)))
                Next
            Catch ex As Exception : End Try
            '-------------------------------------------
            'cria instãncia STMP
            Dim smtp As New SmtpClient()
            smtp.Host = "smtp.gmail.com" ' 'define o servidor SMTP
            smtp.Credentials = New System.Net.NetworkCredential("teste@teste.com.br", "pass") 'Dados do seu servidor SMTP | Suas credenciais SMTP
            smtp.EnableSsl = True 'habilita o envio via SSL
            smtp.Send(mail) 'envia o email
        Catch ex As Exception
            Return False
        End Try
    End Function
    Private Sub Timer3_Tick(sender As Object, e As EventArgs) Handles Timer3.Tick
        Dim url As String = "/webhook"
        ' Criar uma instância do WebClient
        Dim client As New System.Net.WebClient
        Try
            ' Fazer a solicitação GET e obter a resposta como uma string
            client.DownloadString(url)
        Catch ex As Exception

        End Try
    End Sub
End Class
