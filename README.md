Panopto-BatchUploadAPIDemo
=====================

Panopto Batch Upload API Demo

This program uses Panopto RESTful API and C# to accomplish upload to server.

This program will need AWSSDK to run as it uses Amazon S3 Services.

AWSSDK can be downloaded from here: http://aws.amazon.com/s3/

Options for this program is explained below:

BatchUploadAPIDemo.ext SERVER USERNAME USERPASSWORD FOLDERID FILELOCATION FILEPATH [FILEPATH]

	Upload file at FILELOCATION with path as FILEPATH to session with same name as file in folder with ID as FOLDERID on SERVER logged in with USERNAME and USERPASSWORD

	SERVER: Target server
	USERNAME: User name to access the server
	USERPASSWORD: Password for the user
	FOLDERID: ID of the destination folder
	FILELOCATION: Location of file (dn - Disk and Network; ftp - FTP Server; w - HTTP URL)
	FILEPATH: Target upload file path relative to current directory
