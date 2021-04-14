Function UninstallServices
	On Error Resume Next

    Dim APPDIR
    Dim Shell

    Set Shell = CreateObject("WScript.Shell")
       APPDIR = Session.Property("APPDIR")

	Shell.Run APPDIR & "uninstall.bat"

    Set Shell = Nothing
	UninstallServices = 0
End Function
