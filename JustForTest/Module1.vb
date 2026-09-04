Imports System.Timers
Imports System.IO
Imports System.Net.Mail
Imports System.Threading
Imports AIT.ITF
Imports AIT.SM
Imports AIT.RM

Module Module1

    'Interface
    Private Const FILE_CONNECTION_STRING_INTERFACE As String = "cs_interface.xml"
    Private Const FILE_CONNECTION_STRING_CUSTOMS_INTERFACE As String = "cs_customs.xml"
    Private Const APPLICATION_ID_INTERFACE As String = "EDI_Local"

    'Reporter
    Private Const FILE_CONNECTION_STRING_REPORTER As String = "cs_reporter.xml"
    Private Const APPLICATION_ID_REPORTER As String = "Reporter"

    Private oConfig As New Config
    Private oInbound As New Inbound
    Private oProcess As New Proses
    Private oSubmission As New Submission

    Sub Main()
mulai:
        Try
            Console.WriteLine("fun to exec:")
            Dim sModule As String = Console.ReadLine()

            Select Case sModule
                Case "/?"
                    p_ListFunction()
                Case "end"
                    End
                Case "cls"
                    Console.Clear()
                Case "bouncing"
                    p_testEmailBouncing()
                Case "email"
                    p_testSendEmail()
                Case "inbound"
                    p_processFileInbound()
                Case "process"
                    p_processFile()
                Case "submission"
                    p_processSubmission()
                Case "rf"
                    p_rf()
                Case "split"
                    p_split()
                Case "Interface.Getinterval"
                    p_getInterval(FILE_CONNECTION_STRING_INTERFACE)
                Case "Reporter.GetInterval"
                    p_getInterval(FILE_CONNECTION_STRING_REPORTER)
                Case "Reporter.EmailMessage.Process"
                    p_rpt_emailmsg_process()

                Case Else
                    Console.WriteLine("fun not found")
            End Select

            Console.WriteLine("end of fun")
        Catch ex As Exception
            Console.Write(ex)
        End Try

        Console.WriteLine("")
        GoTo mulai
        'Dim oTimer As New System.Timers.Timer
        'oTimer.Interval = 60000 ' 60 seconds 60 000
        'AddHandler oTimer.Elapsed, AddressOf p_exec
        'oTimer.Start()
        'p_processFile()
        'p_testSendEmailBaru()

    End Sub

    Private Sub p_ListFunction()
        Console.WriteLine("bouncing")
        Console.WriteLine("email")
        Console.WriteLine("inbound")
        Console.WriteLine("process")
        Console.WriteLine("split file")
        Console.WriteLine("Getinterval.Interface")
        Console.WriteLine("GetInterval.Reporter")
        Console.WriteLine("Reporter.EmailMessage.Process")
    End Sub

    Private Sub p_rpt_emailmsg_process()
        Dim alEmail As ArrayList = New EmailMessage().GetListByStatus(ProcessStatus.ListProcessStatus.CREATED, FILE_CONNECTION_STRING_REPORTER)

        For Each Data As EmailMessage.DataEmailMessage In alEmail
            'Try
            Dim fileIsNotReady As Boolean = False

            Console.WriteLine(Data.ID)
            Dim alFile As New ArrayList()
            For Each dataAttachment As EmailMessage.DataEmailMessageAttachment In Data.alAttachment
                If File.Exists(dataAttachment.FullFilename) = False Then
                    fileIsNotReady = True
                    Exit For

                End If
                alFile.Add(dataAttachment.FullFilename)
            Next

            Console.WriteLine(fileIsNotReady.ToString)
            If fileIsNotReady = False Then
                Dim en As New EmailNotification
                en.SendEmail(alFile, Data.EmailNotificationID, Data.Subject, Data.Body, FILE_CONNECTION_STRING_REPORTER)

                Dim em As New EmailMessage
                'em.UpdateStatus(Data.ID, ProcessStatus.ListProcessStatus.SENT, FILE_CONNECTION_STRING_REPORTER)

            End If

            'Catch ex As Exception

            'End Try
        Next

    End Sub

    Private Sub p_split()
        Console.WriteLine("fullpath filename?")
        Dim filename As String = Console.ReadLine()
        If filename.Length = 0 Then
            Console.WriteLine("filename cannot be blank")
            Exit Sub
        End If

        Console.WriteLine("line numbers cannot below 100")
        Dim iLine As Int16 = Console.ReadLine()
        If iLine < 100 Then Exit Sub

        Dim reader As StreamReader = New StreamReader(filename)
        Dim fileContent As New ArrayList
        While Not reader.EndOfStream
            fileContent.Add(reader.ReadLine)
        End While
        reader.Close()

        Dim ext As String = Path.GetExtension(filename)
        Dim filenameOnly As String = Path.GetFileNameWithoutExtension(filename)
        Dim dirName As String = Path.GetDirectoryName(filename)

        Dim writer As StreamWriter
        Dim iFile As Int16 = 1
        Dim iCount As Int16 = 1

        For Each sContent As String In fileContent
            If iCount = 1 Then writer = New StreamWriter(String.Format("{0}\{1}_{2}{3}", dirName, filenameOnly, iFile.ToString, ext))
            writer.WriteLine(sContent)
            If iCount = iLine Then
                writer.Close()
                iFile += 1
                iCount = 0
            End If
            iCount += 1
        Next

        writer.Close()
    End Sub

    Private Sub p_rf()
        'System.Diagnostics.Process.Start("remoteRF.vbs")
        Dim foo As New System.Diagnostics.Process
        foo.StartInfo.WorkingDirectory = "c:\"
        'foo.StartInfo.RedirectStandardOutput = True
        foo.StartInfo.FileName = "cmd.exe"
        'foo.StartInfo.Arguments = "%comspec% /C cscript.exe //B //Nologo C:\Users\hgunawan\Documents\Apps\Bin\remoteRF.vbs"
        foo.StartInfo.UseShellExecute = False
        foo.StartInfo.RedirectStandardInput = True
        foo.StartInfo.CreateNoWindow = False
        foo.Start()
        Thread.Sleep(5000)
        Dim s As StreamWriter = foo.StandardInput
        'Dim r As StreamReader = foo.StandardOutput

        s.WriteLine("telnet 10.130.36.10")

        ''foo.StartInfo.UserName = "administrator"
        ''foo.StartInfo.Password = passString
        'foo.Start()
        'r.ReadLine()


        foo.WaitForExit()
        foo.Dispose()
    End Sub

    Private Sub p_exec()
        Dim threadInbound As New Thread(AddressOf p_processFileInbound)
        threadInbound.Start()

        Dim threadProcess As New Thread(AddressOf p_processFile)
        threadProcess.Start()
    End Sub

    Private Sub p_getInterval(fileConnectionString As String)
        Console.WriteLine(String.Format("Get Interval {0}", Now.ToString))
        'Try
        'Dim dConfig As Config.DataConfig = New Config().Get(Config.INTERVAL_SERVICE, fileConnectionString)

        'Console.WriteLine(String.Format("success {0} {1}", Now.ToString, dConfig.valueNumeric.ToString()))
        'Catch ex As Exception
        '    Console.WriteLine(ex)
        'End Try
    End Sub

    Private Sub p_processSubmission()
        Console.WriteLine(String.Format("start submission {0}", Now.ToString))
        'Try
        Dim alException As New ArrayList
        oSubmission.Process(APPLICATION_ID_INTERFACE, FILE_CONNECTION_STRING_INTERFACE, alException)
        If alException.Count > 0 Then
            For Each ex As Exception In alException
                Console.WriteLine(ex)
            Next
        Else
            Console.WriteLine(String.Format("success {0}", Now.ToString))
        End If
        'Catch ex As Exception
        '    Console.WriteLine(ex)
        'End Try
    End Sub

    Private Sub p_processFileInbound()
        Console.WriteLine(String.Format("start Inbound {0}", Now.ToString))
        'Try
        Dim alException As New ArrayList
            oInbound.Process(APPLICATION_ID_INTERFACE, FILE_CONNECTION_STRING_INTERFACE, alException)

            If alException.Count > 0 Then
                For Each ex As Exception In alException
                    Console.WriteLine(ex)
                Next
            Else
                Console.WriteLine(String.Format("success {0}", Now.ToString))
            End If

        'Catch ex As Exception
        '    Console.WriteLine(ex)
        'End Try
    End Sub

    Private Sub p_processFile()
        Console.WriteLine(String.Format("start Process {0}", Now.ToString))
        Try
            Dim alException As New ArrayList
            oProcess.Processing(APPLICATION_ID_INTERFACE, FILE_CONNECTION_STRING_INTERFACE, FILE_CONNECTION_STRING_CUSTOMS_INTERFACE, alException)

            If alException.Count > 0 Then
                For Each ex As Exception In alException
                    Console.WriteLine(ex)
                Next
            Else
                Console.WriteLine(String.Format("success {0}", Now.ToString))
            End If
        Catch ex As Exception
            Console.WriteLine(ex)
        End Try
    End Sub

    Private Sub p_testSendEmail()
        Dim file As String = "Test.log"
        Dim hLink As String = """ftp://10.130.36.15/SAMIFG/FileUploadWMS/" + Path.GetFileName(file) + """"
        Dim sBody As String = String.Empty
        sBody = "<p>Dear All,</p> " +
                "<p>This is auto generated email. <strong>DO NOT REPLY</strong>.</p> " +
                "<p>Please be informed, process as being mentioned in the Subject have completed. Attached is detail information for your reference.</p> " +
                "<p>Click&nbsp;<a href=" + hLink + ">Test</a> to download file.</p> " +
                "<p>&nbsp;</p> " +
                "<p>Regards,</p> " +
                "<p>Agility IT</p>"

        Console.WriteLine(sBody)
        Dim en As New EmailNotification
        en.SendEmail(file, "hg", "(IGNORE) TEST ONLY ", sBody, FILE_CONNECTION_STRING_INTERFACE)
    End Sub

    Private Sub p_testEmailBouncing()
        Dim mail = New MailMessage
        mail.From = New MailAddress("agilityindonesiait@gmail.com")
        mail.To.Add("hgunawan@agility.com")
        mail.CC.Add("oppie.gunawan@yahoo.com.sg")
        mail.Bcc.Add("hendra.oppie@yahoo.com")
        mail.Subject = "test bouncing"
        mail.Priority = MailPriority.High

        'Dim Attachment = New System.Net.Mail.Attachment(File)
        'mail.Attachments.Add(Attachment)
        Dim SmtpServer = New SmtpClient("smtp.gmail.com", 587)
        SmtpServer.EnableSsl = True
        SmtpServer.Credentials = New System.Net.NetworkCredential("agilityindonesiait", "agility1+") 'agility1+ 541291
        SmtpServer.Send(mail)

    End Sub

    Private Sub p_testCsvRempong()

        Dim sm As New SystemManager
        Dim str As String = "32107-TDL-J104 04B0,WIRING HARNESS HONDA FREED W/H FLOOR,BFL7,8.8125,8.64,,HONDA FREED W/H FLOOR,HONDA EXPORT,72.63,,,,,,POLYTAINER,SET,2,20,INDONESIA,,,,\""32107-TDL-J104 04B0,2,2119,B6E6,BFL7,190416\"",PLBSAMTG"

        Dim alVal As ArrayList = sm.SplitCsvQuote(str, ",")

    End Sub

End Module
