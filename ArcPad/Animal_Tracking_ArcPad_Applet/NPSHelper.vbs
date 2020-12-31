option explicit

'Helper Functions

'============================================================================================
'============================================================================================
sub DisableTransectButtons
	


	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransect").Enabled=false
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnStartTransect").Enabled=false
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnLoadMap").Enabled=false
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnMarkAnimal").Enabled=false
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnEndTransect").Enabled=false
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnLand").Enabled=false
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransectDown").Enabled=false
end sub

'============================================================================================
'============================================================================================
function CheckPath(FileNameAndPath)

	Dim ErrorMessage, ThisFile

	set ThisFile = Application.CreateAppObject("File")
	if not ThisFile.Exists(FileNameAndPath) then
   		 CheckPath= "A shapefile is missing with the " & _ 
		"following path and file name:" & FileNameAndPath
		set ThisFile=nothing
		exit function
	end if

	set ThisFile=nothing
	 CheckPath=""

end function


'============================================================================================
'============================================================================================
function OpenLayer(FileNameAndPath)

	Dim ErrorMessage, ThisFile

	set ThisFile = Application.CreateAppObject("File")
	if not ThisFile.Exists(FileNameAndPath) then
   		OpenLayer= "Could not find the Transect shapefile  usng the " & _ 
		"following path and file name:" & FileNameAndPath
		set ThisFile=nothing
		exit function
	end if

	Application.Map.AddLayerFromFile (FileNameAndPath)

	set ThisFile=nothing
	OpenLayer=""

end function

'============================================================================================
'============================================================================================
function AddRecord(DbfPathAndName,FieldName,FieldValue)

	Dim myRS 

	set myRS=Application.CreateAppObject("RecordSet")

	myRS.Open DbfPathAndName,2


	
	if myRS.Fields(FieldName) is nothing then
		myRS.Close
		set myRS=nothing
		AddRecord="The field '" & FieldName & "' does not exist in dbf '" & DbfPathAndName & "'"
		exit function
	end if 

	myRS.AddNew
	myRS.Fields(FieldName).Value=FieldValue
	myRS.Update

	myRS.MoveFirst
	myRS.Close

	set myRS=nothing


end function

'============================================================================================
'============================================================================================
function HasRecords(DbfPathAndName)

	Dim myRS 


	set myRS=Application.CreateAppObject("RecordSet")
	
	myRS.Open DbfPathAndName,1
	

	if myRS.RecordCount=0 then
		HasRecords=false
	else
		HasRecords=true
	end if

	myRS.Close

	set myRS=nothing

end function

'============================================================================================
'============================================================================================
function RecordCount(DbfPathAndName)

	Dim myRS 


	set myRS=Application.CreateAppObject("RecordSet")
	
	myRS.Open DbfPathAndName,1
	
	RecordCount=myRS.RecordCount

	myRS.Close

	set myRS=nothing

end function

'============================================================================================
'============================================================================================
sub DeleteRecord(DbfPathAndName,FieldName,FieldValue)
	
	Dim myRS 


	set myRS=Application.CreateAppObject("RecordSet")
	
	myRS.Open DbfPathAndName,2
	

	if myRS.RecordCount=0 then
		myRS.Close
		set myRS=nothing
		exit sub
	end if
	myRS.MoveFirst

	do while not myRS.EOF

		if myRS.Fields(FieldName).Value=FieldValue then
			myRS.Delete
			myRS.Update
			exit do
		end if
		myRS.MoveNext
	loop
	
	myRS.Pack
	myRS.Close

	if not myRS.RecordCount=0 then
		myRS.MoveFirst
	end if
	set myRS=nothing

end sub

'============================================================================================
'============================================================================================
sub DeleteRecords(DbfPathAndName)
	
	Dim myRS 


	set myRS=Application.CreateAppObject("RecordSet")
	
	myRS.Open DbfPathAndName,2
	

	if myRS.RecordCount=0 then
		myRS.Close
		set myRS=nothing
		exit sub
	end if
	myRS.MoveFirst

	do while not myRS.EOF
		myRS.Delete
		myRS.Update
		myRS.MoveNext
	loop
	
	myRS.Pack
	myRS.Close

	set myRS=nothing

end sub

'============================================================================================
'============================================================================================
function GetGPSPointCollectionFieldValue(DbfPathAndName,FieldName)

	Dim myRS 


	set myRS=Application.CreateAppObject("RecordSet")

	myRS.Open DbfPathAndName,1

	if myRS.RecordCount=0 then
		myRS.Close
		set myRS=nothing
		exit function
	end if
	myRS.MoveFirst

	if not myRS.Fields(FieldName) is nothing then
			GetGPSPointCollectionFieldValue=myRS.Fields(FieldName).Value
			exit function
	end if

	myRS.Close
	set myRS=nothing
	GetGPSPointCollectionFieldValue=""

end function

'============================================================================================
'============================================================================================
function GetRecordsAsArray(DbfPathAndName,FieldName) 

	Dim myRS 
	Dim ThisList()
	dim index


	set myRS=Application.CreateAppObject("RecordSet")

	myRS.Open DbfPathAndName,1


	if myRS.Fields(FieldName) is nothing then
		myRS.Close
		set myRS=nothing
		GetRecordsAsArray=null
		exit function
	end if 

	if myRS.RecordCount=0 then
		myRS.Close
		set myRS=nothing
		GetRecordsAsArray=null
		exit function
	end if

	myRS.MoveFirst
	index=0
	do while not myRS.EOF
		
		if not myRS.Fields(FieldName).IsNull and not myRS.Fields(FieldName)="" then
			redim preserve ThisList(index)
			ThisList(index)=myRS.Fields(FieldName).value
			index=index+1
		else
			
		end if
		myRS.MoveNext
	loop

	myRS.MoveFirst
	myRS.Close	
	set myRS=nothing


	GetRecordsAsArray=ThisList

	
end function

'============================================================================================
'============================================================================================
function GetFieldNamesAsArray(DbfPathAndName) 

	Dim myRS 
	Dim ThisList()
	dim index
	dim FieldCount,FieldTotal

	set myRS=Application.CreateAppObject("RecordSet")

	myRS.Open DbfPathAndName,1
	

	if myRS.Fields.Count=0 then
		myRS.Close
		set myRS=nothing
		GetFieldNamesAsArray=null
		exit function
	end if

	

	FieldCount=1
	FieldTotal=myRS.Fields.Count
	redim preserve ThisList(FieldTotal-1)
	do while FieldCount <= FieldTotal
		
		ThisList(FieldCount-1)=myRS.Fields.Item(FieldCount).Name
		FieldCount=FieldCount + 1	
	loop


	myRS.Close	

	set myRS=nothing

	GetFieldNamesAsArray=ThisList

	
end function

'============================================================================================
'============================================================================================
function GetSelFeatureValue(FieldName)

	dim objSelLayer
	Dim myRS 
	Dim FieldValue

	set objSelLayer = Map.SelectionLayer
	if objSelLayer is nothing then
   		exit function
	end if

	
	set myRS=Map.SelectionLayer.Records
	myRS.Bookmark=Map.SelectionBookmark
			
	if myRS.Fields(FieldName) is nothing then
		GetSelFeatureValue=null
	end if

	if myRS.Fields(FieldName).IsNull or myRS.Fields(FieldName)="" then
		GetSelFeatureValue=null
	end if
		

	FieldValue=myRS.Fields(FieldName).Value
	GetSelFeatureValue=FieldValue


	set myRS=nothing
	set objSelLayer=nothing


end function

'============================================================================================
'============================================================================================
sub RestartGlobalCollectionWithCurrentGPSPoint(X,Y,Z)

	Dim GPSPoint

	set GPSPoints_Collection=Application.CreateAppObject ("Points")
	set GPSPoint=Application.CreateAppObject ("Point")

	GPSPoint.X=X
	GPSPoint.Y=Y
	GPSPoint.Z=Z
	GPSPoints_Collection.Add GPSPoint

	set GPSPoint=nothing
	
end sub

'============================================================================================
'============================================================================================
function MakeLineFromGPSPointsAndAddToLayer(GPSPoint,LayerName,SurveyID,TransectID,SegmentID,SegmentType)

	
	Dim LineSegement
	Dim lyrTrackLog
	Dim ErrorMessage
	dim myRS
	Dim Obs1Dir,Obs1LNam,Obs2LNam,Obs2Dir,PilotDir,PilotLNam,DistanceFromLastPoint

	ErrorMessage=CheckGPS()
	if not ErrorMessage="" then
		MakeLineFromGPSPointsAndAddToLayer= ErrorMessage
		exit function
	end if
	
	if GPSPoints_Collection is nothing then
		MakeLineFromGPSPointsAndAddToLayer="GPS Collection has not been initialized"
		exit function
	end if

	Set LineSegement = Application.CreateAppObject ("Line")

	PilotDir=Application.UserProperties("PilotDir")
	PilotLNam=Application.UserProperties("Pilot")
	Obs1Dir=Application.UserProperties("ObsDir1")
	Obs1LNam=Application.UserProperties("Observer1")

	if not Application.UserProperties("Observer2")="" then
		Obs2LNam=Application.UserProperties("Observer2")
		Obs2Dir=Application.UserProperties("ObsDir2")
	end if
	


	If GPSPoints_Collection.Count > 0 Then

		DistanceFromLastPoint=DistanceFromLastPointInCollection(GPSPoint)

		if DistanceFromLastPoint<1000 then
			GPSPoints_Collection.Add GPSPoint
		end if


	    LineSegement.Parts.Add GPSPoints_Collection

		set lyrTrackLog = Application.Map.Layers("TrackLog")

		if not lyrTrackLog is nothing then

			lyrTrackLog.Editable=true
			Map.AddFeature LineSegement,False

			set myRS=Map.SelectionLayer.Records
			myRS.Bookmark=Map.SelectionBookmark

			if not myRS.Fields("SurveyID") is nothing then
				myRS.Fields("SurveyID").Value=CLng(SurveyID)
			end if
			if not myRS.Fields("TransectID") is nothing then
				myRS.Fields("TransectID").Value=CLng(TransectID)
			end if
			if not myRS.Fields("SegmentID") is nothing then
				myRS.Fields("SegmentID").Value=CLng(SegmentID)
			end if
			if not myRS.Fields("SegType") is nothing then
				myRS.Fields("SegType").Value=SegmentType
			end if


			if not myRS.Fields("PilotLNam") is nothing then
				myRS.Fields("PilotLNam").Value=PilotLNam
			end if
			if not myRS.Fields("PilotDir") is nothing then
				myRS.Fields("PilotDir").Value=PilotDir
			end if
			if not myRS.Fields("Obs1LNam") is nothing then
				myRS.Fields("Obs1LNam").Value=Obs1LNam
			end if
			if not myRS.Fields("Obs1Dir") is nothing then
				myRS.Fields("Obs1Dir").Value=Obs1Dir
			end if			
			
			if not Application.UserProperties("Observer2")="" then
				if not myRS.Fields("Obs2LNam") is nothing then
					myRS.Fields("Obs2LNam").Value=Obs2LNam
				end if
				if not myRS.Fields("Obs2Dir") is nothing then
					myRS.Fields("Obs2Dir").Value=Obs2Dir
				end if			
			end if


			myRS.Update
			lyrTrackLog.Editable=false
			Map.Refresh true
		end if
	  	 
	end if

	Set LineSegement=nothing
	set lyrTrackLog=nothing
	set myRS=nothing
	set GPSPoints_Collection=Application.CreateAppObject ("Points")
	

end function

'============================================================================================
'============================================================================================
function CheckGPS

		'check GPS
	if GPS is nothing then		
		CheckGPS= "ArcPad could not locate the GPS device."
		exit function
	end if
	if GPS.IsOpen=false then
		CheckGPS= "ArcPad could not locate the GPS device."
		exit function
	end if 

	CheckGPS=""

end function

'============================================================================================
'============================================================================================
function GetTransectFlownStatus(SFPathAndName,SurveyID,TransectID)

	dim myRS
	dim FoundRecord
	dim FieldSurveyID,FieldTransectID


	set myRS=Application.CreateAppObject("RecordSet")

	myRS.Open SFPathAndName,1
	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
		GetTransectFlownStatus=""
		myRS.Close
		set myRS=nothing
		exit function
	end if


	FoundRecord=false
	do while not myRS.EOF

		if not myRS.Fields("TransectID") is nothing and not  myRS.Fields("SurveyID") is nothing then
			
			FieldSurveyID=CStr(myRS.Fields("SurveyID").Value)
			FieldTransectID=CStr(myRS.Fields("TransectID").Value)

			if StrComp(TransectID,FieldTransectID)=0 and StrComp(FieldSurveyID,SurveyID)=0  then
				FoundRecord=true
			else
				FoundRecord=false
			end if
		end if

		if FoundRecord=true then

			if  not myRS.Fields("Flown") is nothing then

				GetTransectFlownStatus=myRS.Fields("Flown").Value
				myRS.MoveFirst
				myRS.Close
				set myRS=nothing
				exit function
			end if
		end if

		myRS.MoveNext

	loop
	
	myRS.MoveFirst
	myRS.Close
	set myRS=nothing
	

end function

'============================================================================================
'zoom to a transect on the transect layer
'============================================================================================
function GetTransectFlownStatus_2(SurveyID,TransectID)

	dim TransectLayer,ErrorMessage,myRS,MyBookmark


	ErrorMessage=""
	GetTransectFlownStatus_2=""


	'find the transects layer
	set TransectLayer=Application.Map.Layers("TrnOrig")

	'make sure the layer is good
	if TransectLayer is nothing then
		ErrorMessage="Could not find transects layer"
	end if

	if ErrorMessage="" then
		'get the layer's underlying data
		set myRS=TransectLayer.Records

		'if we have no records, then we can't get the survey id
		if myRS.RecordCount=0 then
			ErrorMessage="No records found in the transects layer"
		end if
	end if

	if ErrorMessage="" then

		'do a find to get a record with the specified transectid
		 MyBookmark=myRS.Find( "[TransectID] =" & TransectID & " and [SurveyID]=" & SurveyID ) 

		'we could not find the record, let the user know
		if MyBookmark=0 then
			ErrorMessage="The transect id" & TransectID & " does not exist in the transect layer"
		end if

	end if

	if ErrorMessage="" then
		myRS.Bookmark=MyBookmark
		GetTransectFlownStatus_2=myRS("Flown")
	end if

	set TransectLayer=nothing
	set myRS=nothing

end function

'============================================================================================
'============================================================================================
sub SetTransectFlownStatus(SFPathAndName,SurveyID,TransectID,FlownStatus)

	dim myRS
	dim FoundRecord
	dim FieldSurveyID,FieldTransectID


	set myRS=Application.CreateAppObject("RecordSet")

	if not Map.Layers("TrnOrig.shp") is nothing then
		Map.Layers("TrnOrig.shp").Editable=false
	end if

	myRS.Open SFPathAndName,2
	

	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
		SetTransectFlownStatus=""
		myRS.Close
		set myRS=nothing
		exit sub
	end if

	FoundRecord=false
	do while not myRS.EOF

		if not myRS.Fields("TransectID") is nothing and not  myRS.Fields("SurveyID") is nothing then
			
			FieldSurveyID=CStr(myRS.Fields("SurveyID").Value)
			FieldTransectID=CStr(myRS.Fields("TransectID").Value)

			if StrComp(TransectID,FieldTransectID)=0 and StrComp(FieldSurveyID,SurveyID)=0  then
				FoundRecord=true
			else
				FoundRecord=false
			end if
		end if

		if FoundRecord=true then

			if  not myRS.Fields("Flown") is nothing then

				myRS.Fields("Flown").Value=FlownStatus
				myRS.Update
				exit do
			end if
		end if

		myRS.MoveNext

	loop
	
	myRS.Close
	set myRS=nothing
	

end sub

'============================================================================================
'============================================================================================
function GetHighestSegmentID(SFPathAndName,SurveyID,TransectID)

	dim myRS
	dim FoundRecord
	dim FieldSurveyID,FieldTransectID
	dim CurrentFieldValue,HighestFieldValue

	set myRS=Application.CreateAppObject("RecordSet")

	myRS.Open SFPathAndName,1
	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
		GetHighestSegmentID=-1
		myRS.Close
		set myRS=nothing
		exit function
	end if

	HighestFieldValue=-1
	FoundRecord=false
	do while not myRS.EOF

		FoundRecord=false

		if not myRS.Fields("TransectID") is nothing and not  myRS.Fields("SurveyID") is nothing then
			
			FieldSurveyID=CStr(myRS.Fields("SurveyID").Value)
			FieldTransectID=CStr(myRS.Fields("TransectID").Value)

			if StrComp(TransectID,FieldTransectID)=0 and StrComp(FieldSurveyID,SurveyID)=0  then
				FoundRecord=true
			end if
		end if

		if FoundRecord=true then

			if not myRS.Fields("SegmentID") is nothing then

				CurrentFieldValue=myRS.Fields("SegmentID").Value

				if CurrentFieldValue>HighestFieldValue then
					HighestFieldValue=CurrentFieldValue
				end if
		
			end if
		end if

		myRS.MoveNext

	loop

	
	myRS.MoveFirst
	myRS.Close
	set myRS=nothing

	GetHighestSegmentID=HighestFieldValue
	
end function

'============================================================================================
'============================================================================================
function GetHighestFieldValue(SFPathAndName,FieldName,CompareType)

	dim myRS
	dim CurrentFieldValue,HighestFieldValue
	dim CurString,HigString


	set myRS=Application.CreateAppObject("RecordSet")

	myRS.Open SFPathAndName,1
	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
			
		if CompareType="string" then	
			GetHighestFieldValue=""
		else
			GetHighestFieldValue=-1
		end if
		myRS.Close
		set myRS=nothing
		exit function	
	end if

	if not myRS.Fields(FieldName) is nothing then
		HighestFieldValue=	myRS.Fields(FieldName).Value
		CurrentFieldValue=myRS.Fields(FieldName).Value	
	end if


	do while not myRS.EOF
		
		if not myRS.Fields(FieldName) is nothing then


			CurrentFieldValue=myRS.Fields(FieldName).Value
				
			if CompareType="string" then
				
				CurString=CStr(CurrentFieldValue)
				HigString= CStr(HighestFieldValue)

				if StrComp(CurString,HigString)>0 then
					HighestFieldValue=CurrentFieldValue
				end if
			end if

			if CompareType="number" then
					

				if CurrentFieldValue>HighestFieldValue then
					HighestFieldValue=CurrentFieldValue
				end if

			end if
		end if

		myRS.MoveNext
	loop

	myRS.Close
	set myRS=nothing
	GetHighestFieldValue=HighestFieldValue


end function

'============================================================================================
'============================================================================================
function AddPointToBackupLog(BackupLogPathAndName,X,Y,Z,SurveyID,TransectID,SegmentID,SegType)

	Dim ErrorMessage, ThisFile
	Dim myRS 

	set ThisFile = Application.CreateAppObject("File")
	if not ThisFile.Exists(BackupLogPathAndName) then
   		AddPointToBackupLog= "Could not find the Log dbf file  usng the " & _ 
		"following path and file name:" & BackupLogPathAndName
		set ThisFile=nothing
		exit function
	end if

	
	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open BackupLogPathAndName,2

	
	myRS.AddNew
	
	if not myRS.Fields("BACKUP_X") is nothing then
		myRS.Fields("BACKUP_X").Value=X
	end if
	if not myRS.Fields("BACKUP_Y") is nothing then
		myRS.Fields("BACKUP_Y").Value=Y
	end if
	if not myRS.Fields("BACKUP_Z") is nothing then
		myRS.Fields("BACKUP_Z").Value=Z
	end if
	if not myRS.Fields("SURVEYID") is nothing then
		myRS.Fields("SURVEYID").Value=SurveyID
	end if
	if not myRS.Fields("TRANSECTID") is nothing then
		myRS.Fields("TRANSECTID").Value=TransectID
	end if
	if not myRS.Fields("SEGTYPE") is nothing then
		myRS.Fields("SEGTYPE").Value=SegType
	end if
	if not myRS.Fields("SEGMENTID") is nothing then
		myRS.Fields("SEGMENTID").Value=SegmentID
	end if


	if not myRS.Fields("Observer1") is nothing then
		if not Application.UserProperties("Observer1")="" then
			myRS.Fields("Observer1").Value=Application.UserProperties("Observer1")
			if not myRS.Fields("ObsDir1") is nothing then
				if not Application.UserProperties("ObsDir1")="" then
					myRS.Fields("ObsDir1").Value=Application.UserProperties("ObsDir1")
				end if
			end if
			
		end if
	end if
	if not myRS.Fields("Observer2") is nothing then
		if not Application.UserProperties("Observer2")="" then
			myRS.Fields("Observer2").Value=Application.UserProperties("Observer2")

			if not myRS.Fields("ObsDir2") is nothing then
				if not Application.UserProperties("ObsDir2")="" then
					myRS.Fields("ObsDir2").Value=Application.UserProperties("ObsDir2")
				end if
			end if
		end if
	end if


	if not myRS.Fields("Pilot") is nothing then
		if not Application.UserProperties("Pilot")="" then
			myRS.Fields("Pilot").Value=Application.UserProperties("Pilot")

			if not myRS.Fields("PilotDir") is nothing then
				if not Application.UserProperties("PilotDir")="" then
					myRS.Fields("PilotDir").Value=Application.UserProperties("PilotDir")
				end if
			end if

		end if
	end if



	myRS.Update

	myRS.Close

	set myRS=nothing


end Function

'============================================================================================
'============================================================================================
function AddBackupLogPointsToTrackLogAsLine(BackupLogPathAndName,TrackLogPathAndName)

	
	Dim ErrorMessage, ThisFile
	Dim myRS,myRSTrackLog 
	Dim Backup_Collection
	Dim Backup_Point
	Dim Backup_Segment
	Dim SurveyID,TransectID,SegType,SegmentID
	Dim Obs1,Obs2,Pilot,Obs1Dir,Obs2Dir,PilotDir

	set Backup_Collection=Application.CreateAppObject ("Points")
	set Backup_Point=Application.CreateAppObject ("Point")
	set Backup_Segment=Application.CreateAppObject ("Line")
	
	set ThisFile = Application.CreateAppObject("File")
	if not ThisFile.Exists(BackupLogPathAndName) then
   		AddBackupLogPointsToTrackLogAsLine= "Could not find the Log dbf file  usng the " & _ 
		"following path and file name:" & BackupLogPathAndName
		set ThisFile=nothing
		exit function
	end if



	if not ThisFile.Exists(TrackLogPathAndName) then
   		AddBackupLogPointsToTrackLogAsLine= "Could not find the TrackLog  Shapefile  usng the " & _ 
		"following path and file name:" & TrackLogPathAndName
		set ThisFile=nothing
		exit function
	end if



	set myRS=Application.CreateAppObject("RecordSet")
	set myRSTrackLog =Application.CreateAppObject("RecordSet")

	myRS.Open BackupLogPathAndName,1
	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
		myRS.Close
		set myRS=nothing
	end if

	if not myRS.EOF then

		if not myRS.Fields("SURVEYID") is nothing then
			SurveyID=myRS.Fields("SURVEYID").Value
		end if
		if not myRS.Fields("TRANSECTID") is nothing then
			TransectID=myRS.Fields("TRANSECTID").Value
		end if
		if not myRS.Fields("SEGTYPE") is nothing then
			SegType=myRS.Fields("SEGTYPE").Value
		end if
		if not myRS.Fields("SEGMENTID") is nothing then
			SegmentID=myRS.Fields("SEGMENTID").Value
		end if

		if not myRS.Fields("OBSERVER1") is nothing then
			Obs1=myRS.Fields("OBSERVER1").Value
			if not myRS.Fields("OBSDIR1") is nothing then
				Obs1Dir=myRS.Fields("OBSDIR1").Value
			end if
		end if

		if not myRS.Fields("OBSERVER2") is nothing then
			Obs2=myRS.Fields("OBSERVER2").Value
			if not myRS.Fields("OBSDIR2") is nothing then
				Obs2Dir=myRS.Fields("OBSDIR2").Value
			end if
		end if

		if not myRS.Fields("PILOT") is nothing then
			Pilot=myRS.Fields("PILOT").Value
			if not myRS.Fields("PILOTDIR") is nothing then
				PilotDir=myRS.Fields("PILOTDIR").Value
			end if
		end if

	end if

	do while not myRS.EOF
		
		if not myRS.Fields("BACKUP_X") is nothing then
			Backup_Point.X = myRS.Fields("BACKUP_X").Value
		end if
		if not myRS.Fields("BACKUP_Y") is nothing then
			Backup_Point.Y = myRS.Fields("BACKUP_Y").Value
		end if
		if not myRS.Fields("BACKUP_Z") is nothing then
			Backup_Point.Z = myRS.Fields("BACKUP_Z").Value
		end if


		Backup_Collection.Add Backup_Point
		myRS.MoveNext
		
	Loop
	myRS.MoveFirst
	myRS.Close




	if Backup_Collection.Count>0 then
			Backup_Segment.Parts.Add Backup_Collection

			myRSTrackLog.Open TrackLogPathAndName,2
			myRSTrackLog.AddNew Backup_Segment

			

			if not myRSTrackLog.Fields("SurveyID") is nothing then
				myRSTrackLog.Fields("SurveyID").Value=CLng(SurveyID)
			end if
			if not myRSTrackLog.Fields("TransectID") is nothing then
				myRSTrackLog.Fields("TransectID").Value=CLng(TransectID)
			end if
			if not myRSTrackLog.Fields("SegType") is nothing then
				myRSTrackLog.Fields("SegType").Value=SegType
			end if
			if not myRSTrackLog.Fields("SegmentID") is nothing then
				myRSTrackLog.Fields("SegmentID").Value=CLng(SegmentID)
			end if

			if not myRSTrackLog.Fields("PilotLNam") is nothing then
				myRSTrackLog.Fields("PilotLNam").Value=Pilot
			end if
			if not myRSTrackLog.Fields("PilotDir") is nothing then
				myRSTrackLog.Fields("PilotDir").Value=PilotDir
			end if
			if not myRSTrackLog.Fields("Obs1LNam") is nothing then
				myRSTrackLog.Fields("Obs1LNam").Value=Obs1
			end if
			if not myRSTrackLog.Fields("Obs1Dir") is nothing then
				myRSTrackLog.Fields("Obs1Dir").Value=Obs1Dir
			end if			
			
			if not Obs2="" then
				if not myRSTrackLog.Fields("Obs2LNam") is nothing then
					myRSTrackLog.Fields("Obs2LNam").Value=Obs2
				end if
				if not myRSTrackLog.Fields("Obs2Dir") is nothing then
					myRSTrackLog.Fields("Obs2Dir").Value=Obs2Dir
				end if			
			end if

			myRSTrackLog.Update
			myRSTrackLog.Close
		
	end if

	set Backup_Point=nothing
	set myRS=nothing
	set myRSTrackLog=nothing
	set Backup_Segment=nothing

end function

'============================================================================================
'============================================================================================
function DeleteAllBackupLogFilePoints(BackupLogPathAndName)

	
	Dim ErrorMessage, ThisFile
	Dim myRS 


	
	set ThisFile = Application.CreateAppObject("File")
	if not ThisFile.Exists(BackupLogPathAndName) then
   		DeleteAllBackupLogFilePoints= "Could not find the Log dbf file  usng the " & _ 
		"following path and file name:" & BackupLogPathAndName
		set ThisFile=nothing
		exit function
	end if


	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open BackupLogPathAndName,2

	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
		myRS.Close
		set myRS=nothing
		exit function
	end if

	do while not myRS.EOF
		
		myRS.Delete
		myRS.Update

		myRS.MoveNext
		
	Loop

	myRS.Pack
	myRS.Close


	set myRS=nothing


end function

'============================================================================================
'============================================================================================
function HasLogFilePoints(BackupLogPathAndName)

	
	Dim ErrorMessage, ThisFile
	Dim myRS 


	
	set ThisFile = Application.CreateAppObject("File")
	if not ThisFile.Exists(BackupLogPathAndName) then
   		HasLogFilePoints= "Could not find the Log dbf file  usng the " & _ 
		"following path and file name:" & BackupLogPathAndName
		set ThisFile=nothing
		exit function
	end if


	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open BackupLogPathAndName,1
	
	

	if  myRS.RecordCount >0  then
		HasLogFilePoints=true
	else
		HasLogFilePoints=false
	end if


	
	myRS.Close
	set myRS=nothing
	set ThisFile =nothing

end function


'============================================================================================
'============================================================================================
function AddLastBackupLogPointToGlobalCollection(BKLogPathAndName)

	Dim ErrorMessage, ThisFile
	Dim myRS 
	Dim TotalLength,CurrentLength
	Dim GPSPoint

	
	set ThisFile = Application.CreateAppObject("File")
	if not ThisFile.Exists(BackupLogPathAndName) then
   		BackUpPointsTransLengthSoFar= "Could not find the Log dbf file  usng the " & _ 
		"following path and file name:" & BackupLogPathAndName
		set ThisFile=nothing
		exit function
	end if


	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open BackupLogPathAndName,2

	if myRS.RecordCount>0 then 
		myRS.MoveLast
	else
		myRS.Close
		set ThisFile =nothing
		set myRS=nothing
		exit function
	end if


	set GPSPoints_Collection=Application.CreateAppObject ("Points")
	set GPSPoint=Application.CreateAppObject ("Point")

	if not myRS.Fields("BACKUP_X") is nothing then
		GPSPoint.X=CDbl(myRS.Fields("BACKUP_X").Value)
	end if
	if not myRS.Fields("BACKUP_Y") is nothing then
		GPSPoint.Y=CDbl(myRS.Fields("BACKUP_Y").Value)
	end if
	if not myRS.Fields("BACKUP_Z") is nothing then
		GPSPoint.Z=CDbl(myRS.Fields("BACKUP_Z").Value)
	end if


	myRS.Close

	GPSPoints_Collection.Add GPSPoint
	set GPSPoint=nothing
	set myRS=nothing
	set ThisFile =nothing
		
end function

'============================================================================================
'============================================================================================
function BackUpPointsTransLengthSoFar(BackupLogPathAndName)

	Dim ErrorMessage, ThisFile
	Dim myRS 
	Dim TotalLength,CurrentLength
	

	
	set ThisFile = Application.CreateAppObject("File")
	if not ThisFile.Exists(BackupLogPathAndName) then
   		BackUpPointsTransLengthSoFar= "Could not find the Log dbf file  usng the " & _ 
		"following path and file name:" & BackupLogPathAndName
		set ThisFile=nothing
		exit function
	end if


	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open BackupLogPathAndName,2

	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
		myRS.Close
		set myRS=nothing
		set ThisFile=nothing
		exit function
	end if

	TotalLength=0
	do while not myRS.EOF
		
		if not myRS.Fields("DISTANCE") is nothing then
			CurrentLength=CDbl(myRS.Fields("DISTANCE").Value)
			TotalLength=TotalLength+CurrentLength
		end if

		myRS.MoveNext
		
	Loop

	BackUpPointsTransLengthSoFar=TotalLength
	myRS.Pack
	myRS.MoveFirst
	myRS.Close

	set ThisFile=nothing
	set myRS=nothing

end function

'============================================================================================
'============================================================================================
function DistanceFromLastPointInCollection(GPSPoint)

	Dim LastCollPoint
	dim Distance

	if GPSPoints_Collection.Count<1 then
		DistanceFromLastPointInCollection=0
		exit function
	end if

	set LastCollPoint=GPSPoints_Collection(GPSPoints_Collection.Count)

	
	Distance=LastCollPoint.DistanceTo(GPSPoint)

	set LastCollPoint=nothing
	
	DistanceFromLastPointInCollection=Distance
	
end function

'============================================================================================
'============================================================================================
function SetDistanceFromLog(DistanceLogPathAndName,Distance)

	Dim ErrorMessage, ThisFile
	Dim myRS 
	Dim Backup_Point
	Dim LastLogPoint

	
	set ThisFile = Application.CreateAppObject("File")
	if not ThisFile.Exists(DistanceLogPathAndName) then
   		SetDistanceFromLog= "Could not find the Log dbf file  usng the " & _ 
		"following path and file name:" & DistanceLogPathAndName
		set ThisFile=nothing
		exit function
	end if


	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open DistanceLogPathAndName,2

	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
		myRS.Close 
		set myRS=nothing
		set ThisFile=nothing
		SetDistanceFromLog=-1
		exit function
	end if

	
	do while not myRS.EOF
		if not myRS.Fields("DISTANCE") is nothing then
			if  not myRS.Fields("DISTANCE").Value="" then
				myRS.Fields("DISTANCE").Value=CDbl(Distance)
				myRS.Update
				myRS.Close
				
				set myRS=nothing
				exit function
			end if
		end if
		myRS.MoveNext
	loop


	SetDistanceFromLog=""
	set ThisFile=nothing
	set myRS=nothing

end function

'============================================================================================
'============================================================================================
function GetDistanceFromLog(DistanceLogPathAndName)

	Dim ErrorMessage, ThisFile
	Dim myRS 
	Dim Backup_Point
	Dim LastLogPoint

	
	set ThisFile = Application.CreateAppObject("File")
	if not ThisFile.Exists(DistanceLogPathAndName) then
   		GetDistanceFromLog= "Could not find the Log dbf file  usng the " & _ 
		"following path and file name:" & DistanceLogPathAndName
		set ThisFile=nothing
		exit function
	end if


	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open DistanceLogPathAndName,1

	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
		myRS.Close
		set ThisFile=nothing
		set myRS=nothing
		SetDistanceFromLog=-1
		exit function
	end if

	do while not myRS.EOF
		if not myRS.Fields("DISTANCE") is nothing then
			if  not myRS.Fields("DISTANCE").Value="" then
				GetDistanceFromLog=myRS.Fields("DISTANCE").Value
				myRS.Close
				set myRS=nothing
				exit function
			end if
		end if
		myRS.MoveNext
	loop

	GetDistanceFromLog=""
	myRS.MoveFirst
	myRS.Close
	set myRS=nothing
	set ThisFile=nothing

end function

'============================================================================================
'============================================================================================
function DistanceTraveledInLogPoints(BackupLogPathAndName)

	Dim ErrorMessage, ThisFile
	Dim myRS 
	Dim Backup_Point
	Dim TotalLength,Distance
	Dim LastLogPoint

	
	set ThisFile = Application.CreateAppObject("File")
	if not ThisFile.Exists(BackupLogPathAndName) then
   		DistanceTraveledInLogPoints= "Could not find the Log dbf file  usng the " & _ 
		"following path and file name:" & BackupLogPathAndName
		set ThisFile=nothing
		exit function
	end if


	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open BackupLogPathAndName,1

	if myRS.RecordCount>0 then 
		myRS.MoveLast
	else
		myRS.Close
		set myRS=nothing
		DistanceTraveledInLogPoints=-1
		exit function
	end if

	if not myRS.Fields("DISTANCE") is nothing then
		DistanceTraveledInLogPoints=CDbl(myRS.Fields("DISTANCE").Value)
	end if

	myRS.Close
	set ThisFile=nothing
	set myRS.Close=nothing

end function

'============================================================================================
'============================================================================================
function DistanceFromLastBackupLogPoint(GPSPoint,BackupLogPathAndName)

	Dim ErrorMessage, ThisFile
	Dim myRS 
	Dim Backup_Point
	Dim TotalLength,Distance
	Dim LastLogPoint

	
	set ThisFile = Application.CreateAppObject("File")
	if not ThisFile.Exists(BackupLogPathAndName) then
   		BackUpPointsTransLengthSoFar= "Could not find the Log dbf file  usng the " & _ 
		"following path and file name:" & BackupLogPathAndName
		set ThisFile=nothing
		exit function
	end if


	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open BackupLogPathAndName,1

	if myRS.RecordCount>0 then 
		myRS.MoveLast
	else
		myRS.Close
		set ThisFile=nothing
		set myRS=nothing
		DistanceFromLastBackupLogPoint=-1
		exit function
	end if

	set LastLogPoint=Application.CreateAppObject ("Point")

	if not myRS.Fields("BACKUP_X") is nothing then
		LastLogPoint.X=CDbl(myRS.Fields("BACKUP_X").Value)
	end if
	if not myRS.Fields("BACKUP_Y") is nothing then
		LastLogPoint.Y=CDbl(myRS.Fields("BACKUP_Y").Value)
	end if
	if not myRS.Fields("BACKUP_Z") is nothing then
		LastLogPoint.Z=CDbl(myRS.Fields("BACKUP_Z").Value)
	end if
	

	Distance=LastLogPoint.DistanceTo(GPSPoint)
	
	DistanceFromLastBackupLogPoint=Distance

	myRS.Close
	set ThisFile=nothing
	set myRS=nothing
	set LastLogPoint=nothing
	
end function

'============================================================================================
'============================================================================================
function GetTransectLength(TransectSFPathAndName,SurveyID,TransectID)

	Dim ErrorMessage, ThisFile
	Dim myRS,FoundRecord
	Dim Backup_Point
	Dim TotalLength,Distance
	Dim LastLogPoint
	dim FieldSurveyID,FieldTransectID
	
	set ThisFile = Application.CreateAppObject("File")
	if not ThisFile.Exists(TransectSFPathAndName) then
   		GetTransectLength= "Could not find the Log dbf file  usng the " & _ 
		"following path and file name:" & TransectSFPathAndName
		set ThisFile=nothing
		exit function
	end if


	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open TransectSFPathAndName,1

	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
		myRS.CLose
		set ThisFile=nothing
		set myRS=nothing
		GetTransectLength=-1
		exit function
	end if

	do while not myRS.EOF

		if not myRS.Fields("TransectID") is nothing and not  myRS.Fields("SurveyID") is nothing then
				
				FieldSurveyID=CStr(myRS.Fields("SurveyID").Value)
				FieldTransectID=CStr(myRS.Fields("TransectID").Value)

				if StrComp(TransectID,FieldTransectID)=0 and StrComp(FieldSurveyID,SurveyID)=0  then
					FoundRecord=true
				else
					FoundRecord=false
				end if
		end if

		if FoundRecord=true then

				if not myRS.Fields("Shape_Leng") is nothing then
					GetTransectLength=myRS.Fields("Shape_Leng").Value
				else
					GetTransectLength=-1
				end if

				exit do
		end if

		myRS.MoveNext

	loop


	myRS.MoveFirst
	myRS.Close
	set ThisFile=nothing
	set myRS=nothing
	
end function


'============================================================================================
'============================================================================================
function SetObservingDirectionForAll(Direction)

	if LCase(Direction)="right" then
		SetControlSelected "frmNewTransect","PAGE1","btnPilotRight","btnPilotLeft"
		SetControlSelected "frmNewTransect","PAGE1","btnRight1","btnLeft1"
		SetControlSelected "frmNewTransect","PAGE1","btnRight2","btnLeft2"
	else
		SetControlSelected "frmNewTransect","PAGE1","btnPilotLeft","btnPilotRight"
		SetControlSelected "frmNewTransect","PAGE1","btnLeft1","btnRight1"
		SetControlSelected "frmNewTransect","PAGE1","btnLeft2","btnRight2"
	end if

end function

'============================================================================================
'============================================================================================
function SetControlSelected(FormName,PageName,ControlName,DisableControlName)

	dim ThisPage
	dim Caption


	if GetControlSelectedState(FormName,PageName,ControlName)=false then

		set ThisPage=Applet.Forms(FormName).Pages(PageName)
		Caption=ThisPage.Controls(ControlName).Text

		Caption="**" & Caption & "**"

		ThisPage.Controls(ControlName).Text=Caption

		ThisPage.Controls(ControlName).Text=Caption

		if not DisableControlName="" then
			SetControlNotSelected FormName,PageName,DisableControlName
		end if
	end if

	set ThisPage=nothing
end function

'============================================================================================
'============================================================================================
function SetControlNotSelected(FormName,PageName,ControlName)

	Dim FirstAsteris,LastAsteris
	dim ThisPage
	dim Caption
	
	
	if GetControlSelectedState(FormName,PageName,ControlName)=true then

		set ThisPage=Applet.Forms(FormName).Pages(PageName)
		Caption=ThisPage.Controls(ControlName).Text

		FirstAsteris=InStr(Caption,"*")
		LastAsteris=InStrRev(Caption,"*")


		if FirstAsteris>0 and LastAsteris>0 then
			Caption=Mid(Caption,FirstAsteris+2,((LastAsteris-3)-FirstAsteris))
		end if

		ThisPage.Controls(ControlName).Text=Caption
	end if

	set ThisPage=nothing

end function

'============================================================================================
'============================================================================================
function GetSelectedControlText(FormName,PageName,ControlName)

	Dim FirstAsteris,LastAsteris
	dim ThisPage
	dim Caption
	
	
	if GetControlSelectedState(FormName,PageName,ControlName)=true then

		set ThisPage=Applet.Forms(FormName).Pages(PageName)
		Caption=ThisPage.Controls(ControlName).Text

		FirstAsteris=InStr(Caption,"*")
		LastAsteris=InStrRev(Caption,"*")


		if FirstAsteris>0 and LastAsteris>0 then
			Caption=Mid(Caption,FirstAsteris+2,((LastAsteris-3)-FirstAsteris))
		end if

		GetSelectedControlText=Caption
	end if

	set ThisPage=nothing

end function

'============================================================================================
'============================================================================================
function GetControlSelectedState(FormName,PageName,ControlName)

	dim ThisPage
	dim Caption


	set ThisPage=Applet.Forms(FormName).Pages(PageName)
	Caption=ThisPage.Controls(ControlName).Text

	if Mid(Caption,1,1)="*" then
		GetControlSelectedState=true
	else
		GetControlSelectedState=false
	end if

	set ThisPage=nothing
	
end function

'============================================================================================
'============================================================================================
function ToggleControlSelected(FormName,PageName,ControlName)

	if GetControlSelectedState(FormName,PageName,ControlName)=true then
		SetControlNotSelected FormName,PageName,ControlName
	else
		SetControlSelected FormName,PageName,ControlName,""
	end if
end function

'============================================================================================
'============================================================================================
function ToggleControlSelectedState(FormName,PageName,ControlName)

	if GetControlSelectedState(FormName,PageName,ControlName)=true then
		SetControlNotSelected FormName,PageName,ControlName
	else
		SetControlSelected FormName,PageName,ControlName,""
	end if

end function

'============================================================================================
'============================================================================================
function GetCurrentDateShort

	Dim ShortYear

	ShortYear=Year(Date)
	ShortYear=Mid(ShortYear,3,4)
	GetCurrentDateShort=Month(Date) & "/" & Day(Date) & "/" & ShortYear

end function

'============================================================================================
'============================================================================================
function GetCurrentTime

	GetCurrentTime=Hour(Time) & ":" & Minute(Time) & ":" & Second(Time)

end function

'============================================================================================
'============================================================================================
function GetCurrentYear

	GetCurrentYear=Year(Date)

end function

'============================================================================================
'============================================================================================
function PopulateComboBox(DBFPathAndName,ListControl,FieldName)

	dim ItemCount,ThisList,ListItem

	ThisList=GetRecordsAsArray(DBFPathAndName,FieldName)


	ListControl.Clear
	if IsNull(ThisList) then
		exit function
	end if


	 ItemCount=0
	for each ListItem in ThisList
	
		ListControl.AddItem ListItem,ListItem
		ItemCount= ItemCount+1

	next
end function

'============================================================================================
'============================================================================================
function DistanceFromLastSegment(GPSPoint)

	Dim SegmentID,DBFPathAndName,ThisFile
	Dim SegShape,myRS
	Dim ClosestPoint,PntsColl,Distance


	DBFPathAndName=Application.Path & "\Applets\Survey\TrackLog.shp"

	'make sure the TrackLog file is there
	set ThisFile = Application.CreateAppObject("File")
	if not ThisFile.Exists(DBFPathAndName) then
   		DistanceFromLastSegment= -1
		set ThisFile=nothing
		exit function
	end if

	'open the recordset
	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open DBFPathAndName,1

	'get the last segment record created
	if myRS.RecordCount>0 then 
		myRS.MoveLast
	else
		myRS.Close
		set ThisFile =nothing
		set myRS=nothing
		DistanceFromLastSegment=-1
		exit function
	end if

	'get the last segment shape from the segment record
	set SegShape=myRS.Fields.Shape


	if SegShape is nothing then
		myRS.Close
		set myRS=nothing
		set ThisFile =nothing
		DistanceFromLastSegment=-1
		exit function
	end if

	'get the point collection from that segment
	set PntsColl=SegShape.Parts.Item(1)
	
	'get the point in the collection that is closest to the input point
	set ClosestPoint=PntsColl.NearestPoint(GPSPoint)

	'get the distance from the closest point in the segment to the input point
	Distance=GPSPoint.DistanceTo(ClosestPoint)
	

	myRS.Close
	set myRS=nothing
	set ThisFile =nothing
			
	DistanceFromLastSegment=Distance
	
	
end function

'============================================================================================
'============================================================================================
Sub DisplayStatusBar(txtTranStatus)

  If Not StatusBar.Visible Then
    StatusBar.Visible = "True"
  End If

  StatusBar.Width(0) = 150
  StatusBar.Text(0) = txtTranStatus
  StatusBar.Style(0) = 134217728
  

End Sub

'============================================================================================
'============================================================================================
function MyUpDown(FormName,PageName,ControlName,Action,UpperLimit,LowerLimit,Increment)

	Dim ThisControl,CurrentText,CurrentValue,FirstClick

	set ThisControl=Applet.Forms(FormName).Pages(PageName).Controls(ControlName)

	CurrentText=ThisControl.Text

	if IsNumeric(CurrentText) then
		CurrentValue=CInt(CurrentText)
		FirstClick=false
	else
		FirstClick=true
		CurrentValue=UpperLimit
	end if

	if FirstClick=true then
		ThisControl.Text=CStr(CurrentValue)
		ThisControl.SetFocus
		set ThisControl=nothing
		exit function
	end if

	if Action="add" then
		if not (CurrentValue+Increment)>UpperLimit then
			ThisControl.Text=CStr(CurrentValue+Increment)
		else
			ThisControl.Text=CStr(LowerLimit)
		end if
	end if

	
	if Action="minus" then
		if not (CurrentValue-Increment)<LowerLimit then
			ThisControl.Text=CStr(CurrentValue-Increment)
		else
			ThisControl.Text=CStr(UpperLimit)
		end if
	end if

	ThisControl.SetFocus

	set ThisControl=nothing

end function

'============================================================================================
'============================================================================================
sub SetDisplayValue(DisplayKey,DisplayValue)

	Dim CurrentText,TextBeforeVal,TextAfterVal
	Dim KeyIndex,ValueStrtIndex,KeyChrLength,TextChrLength,ValueChrLength,SeperatorIndex
	
	
	
	CurrentText=Application.Toolbars("tlbNPSDisplay").Caption


	ValueChrLength=Len(DisplayValue)
	if ValueChrLength=0 then
		exit sub
	end if


	TextChrLength=Len(CurrentText)
	if TextChrLength=0 then
		exit sub
	end if


	KeyChrLength=Len(DisplayKey)
	if KeyChrLength=0 then
		exit sub
	end if


	KeyIndex=Instr(1,CurrentText,DisplayKey,1)
	if KeyIndex=0 then
		exit sub
	end if


	if DisplayKey="Distance Traveled" then
		DisplayValue=AddExtraZerosToStopFlicker(DisplayValue)
	end if


	ValueStrtIndex=KeyIndex+KeyChrLength+1

	
	SeperatorIndex=Instr(ValueStrtIndex,CurrentText,"|",1)
	if SeperatorIndex=0 then
		exit sub
	end if

	TextBeforeVal=Mid(CurrentText,1,ValueStrtIndex)


	TextAfterVal=Mid(CurrentText,SeperatorIndex,TextChrLength)

	CurrentText=TextBeforeVal & DisplayValue & "   " & TextAfterVal 

	if Application.UserProperties("ExtendedTransect")="Extend" and DisplayKey="Distance Traveled" then
		CurrentText=TextBeforeVal & DisplayValue & " (Extended) " & TextAfterVal
	end if

	Application.Toolbars("tlbNPSDisplay").Caption=CurrentText

end sub

'============================================================================================
'============================================================================================
function AddExtraZerosToStopFlicker(ValueString)

	dim ThisValue,SpaceChr,ValChrLength,Count

	SpaceChr=Instr(1,ValueString," ",1)
	if SpaceChr=0 then
		ValueString=ValueString & " "
		SpaceChr=Instr(1,ValueString," ",1)
	end if

	ThisValue=Mid(ValueString,1,SpaceChr-1)

	if Instr(1,ValueString,".",1)=0 then
		ThisValue=ThisValue & ".0"
	end if

	ValChrLength=Len(ThisValue)


	for Count=1 to (5-ValChrLength)
		ThisValue=ThisValue & "0"	
	next
	
	AddExtraZerosToStopFlicker=ThisValue & " km"

end function

'============================================================================================
'============================================================================================
sub ToggleObserveDirToolbarButtons(NewDirection)

		
	
		if LCase(NewDirection)="right" then
			Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotLeft").Visible=false
			Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotRight").Visible=true
		end if

		if LCase(NewDirection)="left" then
			Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotRight").Visible=false
			Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotLeft").Visible=true
		end if

		Call RestartSegementOnViewSideChange

		Application.UserProperties("PilotDir")=NewDirection
		Application.UserProperties("ObsDir1")=NewDirection
		Application.UserProperties("ObsDir2")=NewDirection

end sub

'============================================================================================
'============================================================================================
sub RestartSegementOnViewSideChange

	Dim SurveyID,TransectID,SegmentID,SegType,TransectMode
	Dim GPSPoint,ErrorMessage

	'get the current transect's properties
	TransectID=Application.UserProperties("TransectID")
	SurveyID=Application.UserProperties("SurveyID")


	'get the current segment's properties
	SegType=Application.UserProperties("SegType")
	SegmentID=CLng(Application.UserProperties("SegmentID"))

	TransectMode = Application.UserProperties("Transect_Mode")


	if TransectMode="Start" then


		set GPSPoint=Application.CreateAppObject ("Point")

		GPSPoint.X = GPS.X
		GPSPoint.Y = GPS.Y
		GPSPoint.Z = GPS.Z



		Application.UserProperties("Transect_Mode")="Stop"

		'create the segment
		ErrorMessage=MakeLineFromGPSPointsAndAddToLayer(GPSPoint,"TrackLog",SurveyID,TransectID,CStr(SegmentID),"OnTransect")
		SegmentID=SegmentID + 1
		Application.UserProperties("SegmentID")=SegmentID


		'delete all the backup points and use the current gps point as the first of the new backup points
		DeleteAllBackupLogFilePoints Application.Path & "\Applets\PointLog.dbf"
		AddPointToBackupLog Application.Path & "\Applets\PointLog.dbf",GPSPoint.X, _ 
		GPSPoint.Y,GPSPoint.Z,SurveyID,TransectID,SegmentID,"OnTransect"

		'use the current gps point as the first of the new collection points for the next segment
		RestartGlobalCollectionWithCurrentGPSPoint GPSPoint.X, _ 
		GPSPoint.Y,GPSPoint.Z


		Application.UserProperties("Transect_Mode")="Start"


	end if

end sub

'============================================================================================
'============================================================================================
sub SetListIndexByItemText(ItemText,ListControl)

	dim Count,TotalItems
	if ListControl is nothing then
		exit sub
	end if


	TotalItems=ListControl.ListCount

	
	for Count=0 to TotalItems-1

		ListControl.ListIndex=Count
		if ListControl.Value=ItemText then
			exit sub
		end if

	next

	ListControl.ListIndex=-1

end sub


'============================================================================================
'============================================================================================
function FindIndexByText(ThisArray,ThisValue)

	Dim Count,TotalItems

	TotalItems=UBound(ThisArray)

	
	if TotalItems=0 then
		if ThisValue=ThisArray(0) then
			FindIndexByText=0
		else
			FindIndexByText=-1
		end if
		exit function
	end if

	for Count=0 to TotalItems


		if ThisValue=ThisArray(Count) then
			FindIndexByText=Count
			exit function
		end if
	next

	
end function

'============================================================================================
'============================================================================================
function ValidateBearFields(ThisArray)

	Dim Count,TotalItems,AnimalBookmark,myRS,ThisLayer
	Dim ErrorMessage,AnimalID,HasRep

	ValidateBearFields=""
	ErrorMessage=""

	if IsArray(ThisArray)=false then
		exit function
	end if


	set ThisLayer = Application.Map.Layers("animals")
	ThisLayer.Editable=true

	TotalItems=UBound(ThisArray)

	
	set myRS=ThisLayer.Records
	

	for Count=0 to TotalItems

		ErrorMessage=""
		AnimalBookmark=ThisArray(Count) 

		if IsNumeric(AnimalBookmark)=true then



		myRS.Bookmark=CLng(AnimalBookmark)

		AnimalID=myRS.Fields("AnimalID").Value
		
		HasRep=false

		if not myRS.Fields("PILOTLNAM") is nothing then
			if not myRS.Fields("PILOTLNAM").IsNull and not myRS.Fields("PILOTLNAM")=""  then

				HasRep=true

				if not myRS.Fields("PILOTREPT") is nothing then
					if myRS.Fields("PILOTREPT").IsNull or myRS.Fields("PILOTREPT")=-1 then
						ErrorMessage=ErrorMessage & ", Pilot Repeatability"
					end if
				end if

			end if
		end if

		if not myRS.Fields("OBS1LNAM") is nothing then
			if not myRS.Fields("OBS1LNAM").IsNull and not myRS.Fields("OBS1LNAM")=""  then

				HasRep=true

				if not myRS.Fields("OBS1REPT") is nothing then
					if myRS.Fields("OBS1REPT").IsNull or myRS.Fields("OBS1REPT")=-1 then
						ErrorMessage=ErrorMessage & ", Observer Repeatability"
					end if
				end if

			end if
		end if

		if HasRep=false then
			ErrorMessage=ErrorMessage & ", Repeatability"
		end if


		if not myRS.Fields("SPECIES") is nothing then
			if myRS.Fields("SPECIES").IsNull or myRS.Fields("SPECIES")=""  then
				ErrorMessage=ErrorMessage & ", Species"
			end if
		end if


		if not myRS.Fields("ANIMALTYPE") is nothing then
			if myRS.Fields("ANIMALTYPE").IsNull or myRS.Fields("ANIMALTYPE")=""  then
				ErrorMessage=ErrorMessage & ", Animal Type"
			end if
		end if

		if not myRS.Fields("GROUPSIZE") is nothing then
			if myRS.Fields("GROUPSIZE").IsNull  or myRS.Fields("GROUPSIZE")<1  then
				ErrorMessage=ErrorMessage & ", (Group Size cannot be zero)"
			end if
		end if

		if not myRS.Fields("GROUPTYPE") is nothing then
			if myRS.Fields("GROUPTYPE").IsNull or myRS.Fields("GROUPTYPE")=""  then
				ErrorMessage=ErrorMessage & ", Group Type"
			end if
		end if

		if not myRS.Fields("ACTIVITY") is nothing then
			if myRS.Fields("ACTIVITY").IsNull or myRS.Fields("ACTIVITY")=""  then
				ErrorMessage=ErrorMessage & ", Activity"
			end if
		end if

		if not myRS.Fields("PCTCOVER") is nothing then
			if myRS.Fields("PCTCOVER").IsNull or myRS.Fields("PCTCOVER")=-1  then
				ErrorMessage=ErrorMessage & ", Percent Cover"
			end if
		end if

		if not myRS.Fields("PCTSNOW") is nothing then
			if myRS.Fields("PCTSNOW").IsNull or myRS.Fields("PCTSNOW")=-1  then
				ErrorMessage=ErrorMessage & ", Percent Snow"
			end if
		end if

		

		if not ErrorMessage="" then

			ErrorMessage=Mid(ErrorMessage,2)
			
			ValidateBearFields= ValidateBearFields & "Animal with ID " & AnimalID _
				& " is missing data or has errors: " & vbCrLf & ErrorMessage  & vbCrLf & vbCrLf
		end if


		end if
	
	next


	ThisLayer.Editable=false
	set ThisLayer=nothing
	set myRS=nothing
	
end function

'============================================================================================
'============================================================================================
function ValidateSheepFields(ThisArray)

	Dim Count,TotalItems,AnimalBookmark,myRS,ThisLayer
	Dim ErrorMessage,AnimalID,HasRep

	ValidateSheepFields=""
	ErrorMessage=""

	if IsArray(ThisArray)=false then
		exit function
	end if

	set ThisLayer = Application.Map.Layers("animals")
	ThisLayer.Editable=true

	TotalItems=UBound(ThisArray)

	
	set myRS=ThisLayer.Records
	

	for Count=0 to TotalItems

		ErrorMessage=""
		AnimalBookmark=ThisArray(Count) 


		if IsNumeric(AnimalBookmark)=true then


		myRS.Bookmark=CLng(AnimalBookmark)

		AnimalID=myRS.Fields("AnimalID").Value
	
		if not myRS.Fields("ACTIVITY") is nothing then
			if myRS.Fields("ACTIVITY").IsNull or myRS.Fields("ACTIVITY")=""  then
				ErrorMessage=ErrorMessage & ", (missing Activity)" & vbCrLf
			end if
		end if

		if not myRS.Fields("TOTAL") is nothing then
			if myRS.Fields("TOTAL").IsNull or myRS.Fields("TOTAL")=0  then
				ErrorMessage=ErrorMessage & ", (TOTAL cannot be zero)"  & vbCrLf
			end if
		end if

		if not ErrorMessage="" then

			ErrorMessage=Mid(ErrorMessage,2)

			ValidateSheepFields= ValidateSheepFields & "Animal with ID " _
				& AnimalID & " has errors: " & vbCrLf & ErrorMessage  & vbCrLf & vbCrLf
		end if


		end if
	
	next


	ThisLayer.Editable=false
	set ThisLayer=nothing
	set myRS=nothing
	
end function

'============================================================================================
'============================================================================================
sub DrawRectOnSelAnimal(AnimalBookmark)

	Dim pRS,pRect
	Dim pSymbol,pLyr
	Dim RecShape
	


	if Map.Layers("animals") is nothing then
		exit sub
	end if


	set pLyr=Map.Layers("animals")
	'pLyr.Editable=true

	 'Create a symbol
 	Set pSymbol = Application.CreateAppObject("symbol") 
    pSymbol.LineColor = apCyan
    pSymbol.LineWidth = 2 
    pSymbol.BackgroundMode =apFillTransparent
	pSymbol.FillColor=apWhite
	pSymbol.FillStyle=apFillEmpty
	pSymbol.BackgroundColor=apWhite

	 'Get selected feature's extent
    Set pRS = pLyr.Records
    pRS.Bookmark = CLng(AnimalBookmark)
      

	set RecShape=pRS.Fields.Shape.Buffer(50)
	Set pRect = RecShape.Extent
	Call Map.DrawShape(pRect, pSymbol) 
	
	'Map.Redraw true
	'pLyr.Editable=false
	set pLyr=nothing
	Set pRS=nothing
	Set pSymbol =nothing
	set RecShape=nothing
	Set pRect =nothing

end sub

'============================================================================================
'get the bookmark for the horizon record matching these field values
'============================================================================================
function GetHorizonBookmark(HorizonID,TransectID,SurveyID)

    dim myRS
	dim FoundRecord
	dim FieldSurveyID,FieldTransectID,FieldHorizonID

	if  not CheckPath(Application.Path & "\Applets\Survey\Horizon.shp")="" then
		GetHorizonReferenceCount=-1
	end if


	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open Application.Path & "\Applets\Survey\Horizon.shp",1

	'if Map.Layers("Horizon") is nothing then
	'	GetHorizonBookmark=-1
	'	exit function
	'end if


	'set myRS=Map.Layers("Horizon").Records

	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
		GetHorizonBookmark=-1
		set myRS=nothing
		exit function
	end if
	
	
	do while not myRS.EOF

		if not myRS.Fields("TransectID") is nothing and not  myRS.Fields("SurveyID") is nothing _ 
		and not  myRS.Fields("HorizonID") is nothing then
			
			FieldSurveyID=CStr(myRS.Fields("SurveyID").Value)
			FieldTransectID=CStr(myRS.Fields("TransectID").Value)
			FieldHorizonID=CStr(myRS.Fields("HorizonID").Value)

	

			if StrComp(TransectID,FieldTransectID)=0 and StrComp(FieldSurveyID,SurveyID)=0 _
			and StrComp(FieldHorizonID,HorizonID)=0 then
				GetHorizonBookmark=myRS.Bookmark
				set myRS=nothing
				exit function
			end if
		end if

		myRS.MoveNext

	loop

	myRS.MoveFirst
	GetHorizonBookmark=-1
	set myRS=nothing
	

end function

'============================================================================================
'How many animal sightings are linked to this horizon
'============================================================================================
function GetHorizonReferenceCount(HorizonID,TransectID,SurveyID)

	dim myRS
	dim TotalRecords
	dim FieldSurveyID,FieldTransectID,FieldHorizonID

	if  not CheckPath(Application.Path & "\Applets\Survey\animals.shp")="" then
		GetHorizonReferenceCount=-1
	end if


	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open Application.Path & "\Applets\Survey\animals.shp",1

	'if Map.Layers("animals") is nothing then
	'	GetHorizonReferenceCount=-1
	'	exit function
	'end if


	'set myRS=Map.Layers("animals").Records
	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
		GetHorizonReferenceCount=-1
		set myRS=nothing
		exit function
	end if
	
	TotalRecords=0
	do while not myRS.EOF

		if not myRS.Fields("TransectID") is nothing and not  myRS.Fields("SurveyID") is nothing _ 
		and not  myRS.Fields("HorizonID") is nothing then
			
			FieldSurveyID=CStr(myRS.Fields("SurveyID").Value)
			FieldTransectID=CStr(myRS.Fields("TransectID").Value)
			FieldHorizonID=CStr(myRS.Fields("HorizonID").Value)


			if StrComp(TransectID,FieldTransectID)=0 and StrComp(FieldSurveyID,SurveyID)=0 _
			and StrComp(FieldHorizonID,HorizonID)=0 then
				TotalRecords=TotalRecords+1
			end if
		end if

		myRS.MoveNext

	loop
	
	GetHorizonReferenceCount=TotalRecords
	myRS.MoveFirst
	set myRS=nothing

end function

'============================================================================================
'============================================================================================
function AddGPSPointToGPSLog(DbfPathAndName,ThisPoint)

	Dim myRS,SurveyID,Longitude,Latitude,Altitude

	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open DbfPathAndName,2

 	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	end if

	Altitude=GPS.Altitude
	Latitude=GPS.Latitude
	Longitude=GPS.Longitude

	myRS.AddNew ThisPoint


	if not myRS.Fields("DATE_") is nothing then
		myRS.Fields("DATE_").Value=GetCurrentDateShort()
	end if
	if not myRS.Fields("TIME_") is nothing then
		myRS.Fields("TIME_").Value=GetCurrentTime()
	end if
	if not myRS.Fields("ALTITUDE") is nothing then
		myRS.Fields("ALTITUDE").Value=Altitude
	end if
	if not myRS.Fields("XCOORD") is nothing then
		myRS.Fields("XCOORD").Value=ThisPoint.X
	end if
	if not myRS.Fields("YCOORD") is nothing then
		myRS.Fields("YCOORD").Value=ThisPoint.Y
	end if
	if not myRS.Fields("LATITUDE") is nothing then
		myRS.Fields("LATITUDE").Value=Latitude
	end if
	if not myRS.Fields("LONGITUDE") is nothing then
		myRS.Fields("LONGITUDE").Value=Longitude
	end if
	if not myRS.Fields("PDOP") is nothing then
		myRS.Fields("PDOP").Value=GPS.Properties("PDOP")
	end if
	if not myRS.Fields("PLANESPD") is nothing then
		myRS.Fields("PLANESPD").Value=GPS.Properties("SOG")
	end if
	if not myRS.Fields("SurveyID") is nothing then
		if not Application.UserProperties("SurveyID")="" then
		myRS.Fields("SurveyID").Value=Application.UserProperties("SurveyID")
		end if
	end if
	if not myRS.Fields("PILOTLNAM") is nothing then
		if not Application.UserProperties("Pilot")="" then
		myRS.Fields("PILOTLNAM").Value=Application.UserProperties("Pilot")
		end if
	end if
	if not myRS.Fields("AIRCRAFT") is nothing then
		if not Application.UserProperties("Aircraft")="" then
		myRS.Fields("AIRCRAFT").Value=Application.UserProperties("Aircraft")
		end if
	end if

	
	myRS.Update

	myRS.MoveFirst
	myRS.Close

	set myRS=nothing


end function


'============================================================================================
'============================================================================================
function ResetAnimalBookmarkArrayToSegementsAnimals(SegmentID,TransectID,SurveyID)

	dim myRS
	dim FieldSurveyID,FieldTransectID,FieldSegmentID


	if  not CheckPath(Application.Path & "\Applets\Survey\animals.shp")="" then
		exit function
	end if

	
	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open Application.Path & "\Applets\Survey\animals.shp",1


	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
		set myRS=nothing
		exit function
	end if
	
	redim AnimalBKPerOffSegment(0)
	AnimalBKPerOffSegment(0)=""

	do while not myRS.EOF

		if not myRS.Fields("TransectID") is nothing and not  myRS.Fields("SurveyID") is nothing _ 
		and not  myRS.Fields("SegmentID") is nothing then
			
			FieldSurveyID=CStr(myRS.Fields("SurveyID").Value)
			FieldTransectID=CStr(myRS.Fields("TransectID").Value)
			FieldSegmentID=CStr(myRS.Fields("SegmentID").Value)


			if StrComp(TransectID,FieldTransectID)=0 and StrComp(FieldSurveyID,SurveyID)=0 _
			and StrComp(FieldSegmentID,SegmentID)=0 then


				if AnimalBKPerOffSegment(0)="" then
					AnimalBKPerOffSegment(UBound(AnimalBKPerOffSegment))=CStr(myRS.Bookmark)
				else
					redim preserve AnimalBKPerOffSegment(UBound(AnimalBKPerOffSegment)+1)
					AnimalBKPerOffSegment(UBound(AnimalBKPerOffSegment))=CStr(myRS.Bookmark)
				end if


			end if
		end if

		

		myRS.MoveNext

	loop
	

	myRS.MoveFirst
	myRS.Close
	set myRS=nothing


end function

'============================================================================================
'============================================================================================
function GetLastAnimalIDForTransect(TransectID,SurveyID)

	dim myRS
	dim FieldSurveyID,FieldTransectID,FieldSegmentID
	dim HighestAnimalID,ThisAnimalID

	HighestAnimalID=-1

	if  not CheckPath(Application.Path & "\Applets\Survey\animals.shp")="" then
		GetLastAnimalIDForTransect=HighestAnimalID
		exit function
	end if

	
	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open Application.Path & "\Applets\Survey\animals.shp",1


	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
		GetLastAnimalIDForTransect=HighestAnimalID
		set myRS=nothing
		exit function
	end if
	

	do while not myRS.EOF

		if not myRS.Fields("TransectID") is nothing and not  myRS.Fields("SurveyID") is nothing then
			
			FieldSurveyID=CStr(myRS.Fields("SurveyID").Value)
			FieldTransectID=CStr(myRS.Fields("TransectID").Value)

			'the animalid count should continue from the last id for the entire transect
			if StrComp(TransectID,FieldTransectID)=0 and StrComp(FieldSurveyID,SurveyID)=0 then
				ThisAnimalID=myRS.Fields("AnimalID").Value
				if ThisAnimalID>HighestAnimalID then
					HighestAnimalID=ThisAnimalID
				end if
			end if

		end if

		myRS.MoveNext

	loop
	

	myRS.MoveFirst
	myRS.Close
	set myRS=nothing


	GetLastAnimalIDForTransect=HighestAnimalID
	

end function

'============================================================================================
'get default value from defaultvalues.dbf
'============================================================================================
function GetDefaultValue(ValueName)

	dim myRS
	dim ValueText,CurValueName


	set myRS=Application.CreateAppObject("RecordSet")
	myRS.Open Application.Path & "\Applets\Survey\defaultvalues.dbf",1


	if myRS.RecordCount>0 then 
		myRS.MoveFirst
	else
		GetDefaultValue=""
		set myRS=nothing
		exit function
	end if

	if myRS.Fields("DEFNAME") is nothing or myRS.Fields("DEFVALUE") is nothing then
		GetDefaultValue=""
		set myRS=nothing
		exit function
	end if
	
	ValueText=""
	do while not myRS.EOF

		CurValueName=CStr(myRS.Fields("DEFNAME").Value)
		if CurValueName=ValueName then
			ValueText=CStr(myRS.Fields("DEFVALUE").Value)
		end if		
		myRS.MoveNext

	loop
	

	myRS.MoveFirst
	myRS.Close
	set myRS=nothing


	GetDefaultValue=ValueText
	
end function


'============================================================================================
'get the surveyid from the transects layer
'============================================================================================
function GetSurveyID()

	dim TransectLayer
	dim myRS
	dim SurveyID

	SurveyID=""

	'find the transects layer
	set TransectLayer=Application.Map.Layers("TrnOrig")

	'make sure the layer is good
	if TransectLayer is nothing then
		GetSurveyID=""
		exit function
	end if

	'get the layer's underlying data
	set myRS=TransectLayer.Records

	'if we have no records, then we can't get the survey id
	if myRS.RecordCount=0 then
		GetSurveyID=""
		exit function
	end if
	
	'move to the first record and get the surveyid from it
	myRS.MoveFirst
	SurveyID=myRS.Fields("SurveyID").Value

	'clear memory
	set myRS=nothing
	set TransectLayer=nothing
	
	'return the surveyid
	GetSurveyID=SurveyID

end function

'============================================================================================
'============================================================================================
function GetFeatureValue(LayerName,FeatureBK,AttrName)

	dim lyThisLayer,AllOk,myRS,AttrValue

	AllOk=true
	AttrValue=""

	'get the layer
	set lyThisLayer=Application.Map.Layers(LayerName)
	if lyThisLayer is nothing then
		GetFeatureValue=""
		AllOk=false
	end if

	'make sure the bookmark is a number
	if AllOk=true then
		if not IsNumeric(FeatureBK) then
			GetFeatureValue=""
			AllOk=false
		end if
	end if

	'we have the layer, get the record
	if AllOk=true then

		set myRS=lyThisLayer.Records
		myRS.Bookmark=CLng(FeatureBK)

		if myRS.BOF and myRS.EOF then
			GetFeatureValue=""
			AllOk=false	
		end if

	end if

	'we have the record- get the attribute value
	if AllOk=true then
		if not myRS.Fields(AttrName) is nothing then
			AttrValue=myRS.Fields(AttrName) 
		end if
	end if

	GetFeatureValue=AttrValue
	set myRS=nothing
	set lyThisLayer=nothing

end function

'============================================================================================
'============================================================================================
function GetFeatureValueByFilter(LayerName,Filter,AttrName)

	dim lyThisLayer,AllOk,myRS,AttrValue,Foundbookmark

	AllOk=true
	AttrValue=""

	'get the layer
	set lyThisLayer=Application.Map.Layers(LayerName)
	if lyThisLayer is nothing then
		GetFeatureValueByFilter=""
		AllOk=false
	end if

	'make sure the filter is okay
	if AllOk=true then
		if Filter="" then
			GetFeatureValueByFilter=""
			AllOk=false
		end if
	end if

	'we have the layer, get the record
	if AllOk=true then

	
		set myRS=lyThisLayer.Records

		on error resume next
		Foundbookmark=myRS.Find(Filter) 
		if  Err.Number <> 0  then 
			GetFeatureValueByFilter=""
			AllOk=false	
		end if
		On Error Goto 0

	end if

	'we have the record- get the attribute value
	if AllOk=true then
		if not myRS.Fields(AttrName) is nothing then
			AttrValue=myRS.Fields(AttrName) 
		end if
	end if

	GetFeatureValueByFilter=AttrValue
	set myRS=nothing
	set lyThisLayer=nothing

end function


'============================================================================================
'zoom to a transect on the transect layer
'============================================================================================
function IsATransect(TransectID)

	dim TransectLayer,ErrorMessage,myRS,MyBookmark


	IsATransect=""


	'find the transects layer
	set TransectLayer=Application.Map.Layers("TrnOrig")

	'make sure the layer is good
	if TransectLayer is nothing then
		ErrorMessage="Could not find transects layer"
		IsATransect=ErrorMessage
		exit function
	end if

	'get the layer's underlying data
	set myRS=TransectLayer.Records

	'if we have no records, then we can't get the survey id
	if myRS.RecordCount=0 then
		ErrorMessage="No records found in the transects layer"
		IsATransect=ErrorMessage
		exit function
	end if

	'do a find to get a record with the specified transectid
	 MyBookmark=myRS.Find( "[TransectID] =" & TransectID ) 

	'we could not find the record, let the user know
	if MyBookmark=0 then
		ErrorMessage="The transect id" & TransectID & " does not exist in the transect layer"
		IsATransect=ErrorMessage
		exit function
	end if

	set TransectLayer=nothing
	set myRS=nothing

end function

'============================================================================================
'zoom to a transect on the transect layer
'============================================================================================
function ZoomToTransect(TransectID)

	dim TransectLayer
	dim myRS
	dim MyBookmark
	dim ErrorMessage
	dim TransectExtent

	ErrorMessage=""

	'find the transects layer
	set TransectLayer=Application.Map.Layers("TrnOrig")

	'make sure the layer is good
	if TransectLayer is nothing then
		ErrorMessage="could not find transects layer"
		ZoomToTransect=ErrorMessage
		exit function
	end if

	'get the layer's underlying data
	set myRS=TransectLayer.Records

	'if we have no records, then we can't get the survey id
	if myRS.RecordCount=0 then
		ErrorMessage="no records found in the transects layer"
		ZoomToTransect=ErrorMessage
		exit function
	end if

	'do a find to get a record with the specified transectid
	 MyBookmark=myRS.Find( "[TransectID] =" & TransectID ) 

	'we could not find the record, let the user know
	if MyBookmark=0 then
		ErrorMessage="the transect id" & TransectID & " does not exist in the transect layer"
		ZoomToTransect=ErrorMessage
		exit function
	end if

	'move to the found transect and then set the map's extent to ithe records's shape extent 
	myRS.Bookmark=MyBookmark
	set TransectExtent=myRS.Fields.Shape.Extent
	TransectExtent.ScaleRectangle(1.5)
	Application.Map.Extent=TransectExtent

	set TransectExtent=nothing
	set TransectLayer=nothing
	set myRS=nothing

end function

'============================================================================================
'============================================================================================
function HighlightTransect(TransectID,DoCentering)

	dim TransectLayer
	dim myRS
	dim MyBookmark
	dim ErrorMessage

	ErrorMessage=""

	'find the transects layer
	set TransectLayer=Application.Map.Layers("TrnOrig")

	'make sure the layer is good
	if TransectLayer is nothing then
		ErrorMessage="could not find transects layer"
		HighlightTransect=ErrorMessage
		exit function
	end if

	'get the layer's underlying data
	set myRS=TransectLayer.Records

	'if we have no records, then we can't get the survey id
	if myRS.RecordCount=0 then
		ErrorMessage="no records found in the transects layer"
		HighlightTransect=ErrorMessage
		set myRS=nothing
		set TransectLayer=nothing
		exit function
	end if

	'do a find to get a record with the specified transectid
	 MyBookmark=myRS.Find( "[TransectID] =" & TransectID ) 

	'we could not find the record, let the user know
	if MyBookmark=0 then
		ErrorMessage="the transect id" & TransectID & " does not exist in the transect layer"
		HighlightTransect=ErrorMessage
		set myRS=nothing
		set TransectLayer=nothing
		exit function
	end if

	if TransectLayer.CanEdit=true then
		TransectLayer.Editable=true
	end if

	'select the transect
	Application.Map.Select TransectLayer,MyBookmark

	if DoCentering=true then
		'center map at transect
		myRS.Bookmark=MyBookmark
		Application.Map.CenterAt myRS.Fields.Shape.Extent.Center
	end if
	

	set myRS=nothing
	set TransectLayer=nothing
	HighlightTransect=""

end function

'============================================================================================
'============================================================================================
function GetTransectAnimalBookmarks(TransectID,AnimalID,byref AnimalBK,byref TransectAnimals)

	dim lyAnimal,myRS,AllOk,Curbookmark,CurAnimalID,AnimalCount
	Dim ErrorMessage

	redim TransectAnimals(0)
	TransectAnimals(0)=""

	AnimalCount=0
	AllOk=true
	AnimalBK=-1
	ErrorMessage=""



	'find layer
	set lyAnimal=Application.Map.Layers("animals")
	if lyAnimal is nothing then
		ErrorMessage="could not find the animals layer"
		AllOk=false
	end if

	if AllOk=true then

		'get the layer's underlying data
		set myRS=lyAnimal.Records
		if myRS.RecordCount=0 then
			ErrorMessage="no records found in the animals layer"
			AllOk=false
		end if

	end if

	if AllOk=true then

		'get all the animals that have transect id of the specified transect id
		Curbookmark=myRS.Find( "[TransectID] =" & TransectID ) 
		do while not Curbookmark= 0

			AnimalCount=AnimalCount+1

			if TransectAnimals(0)="" then
				TransectAnimals(UBound(TransectAnimals))=CStr(Curbookmark)
			else
				redim preserve TransectAnimals(UBound(TransectAnimals)+1)
				TransectAnimals(UBound(TransectAnimals))=CStr(Curbookmark)
			end if

			CurAnimalID=myRS.Fields("AnimalID")

			if IsNumeric(AnimalID) then
				if AnimalID=CStr(CurAnimalID) then AnimalBK=Curbookmark
			end if

			
			Curbookmark=myRS.Find( "[TransectID] =" & TransectID,nothing,Curbookmark) 

		loop
		
		if AnimalCount=0 then
			ErrorMessage="No sightings found for the specified transect."
		end if

	end if

	GetTransectAnimalBookmarks=ErrorMessage

	set myRS=nothing
	set lyAnimal=nothing


end function

'============================================================================================
'============================================================================================
class GPSFinder

	public X
	public Y
	public Z
	public Latitude
	public Longitude
	public PDOP
	public Altitude
	public SOG
	public IsLive

    Private Sub Class_Initialize
		IsLive=false
    End Sub

	public function LoadGPSValues

		if IsLive=true then

			'check GPS
			if GPS is nothing then		
				LoadGPSValues= "ArcPad could not locate the GPS device."
				exit function
			end if
			if GPS.IsOpen=false then
				LoadGPSValues= "ArcPad could not locate the GPS device."
				exit function
			end if 

			'get gps values live
			X=GPS.X
			Y=GPS.Y
			Z=GPS.Z
			Altitude=GPS.Altitude
			Latitude=GPS.Latitude
			Longitude=GPS.Longitude
			PDOP=GPS.Properties("PDOP")
			SOG=GPS.Properties("SOG")
		else
			'set fake values
			X=-130.7009888
			Y=55.57128906
			Z=1000
			Altitude=1000
			Latitude=1000
			Longitude=1000
			PDOP=1000
			SOG=1000		
		end if

		LoadGPSValues=""
	
	end  function	

end class
