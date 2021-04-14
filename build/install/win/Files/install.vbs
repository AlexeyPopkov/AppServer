Function InstallServices
	On Error Resume Next

    Dim APPDIR
    Dim Shell

    Set Shell = CreateObject("WScript.Shell")
       APPDIR = Session.Property("APPDIR")

	Shell.Run APPDIR & "install.bat"

    Set Shell = Nothing
	InstallServices = 0
End Function
