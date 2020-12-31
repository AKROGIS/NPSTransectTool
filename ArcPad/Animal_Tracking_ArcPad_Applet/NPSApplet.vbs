option explicit

'GLobals
Dim GPSPoints_Collection
Dim AnimalBKPerOffSegment
Dim MyGPS
dim DefaultValuesPath
Dim FullExtent,FullExtentScale

DefaultValuesPath=Application.Path & "\Applets\Survey\defaultvalues.dbf"

set GPSPoints_Collection=Application.CreateAppObject ("Points")
redim AnimalBKPerOffSegment(0)
AnimalBKPerOffSegment(0)=""


'============================================================================================
'============================================================================================
sub NPSApplet_Load

	set MyGPS=new GPSFinder
	MyGPS.IsLive=true


	
	'The Trans form pops up when the loadmap button is pressed. The values
	'set in Trans will be saved to these variables for use later on
	Application.UserProperties("SurveyID")=""
	Application.UserProperties("SurveyName")=""
	Application.UserProperties("Flown")=""
	Application.UserProperties("FlownDate")=""

	Application.UserProperties("ObsDir1")="Left"
	Application.UserProperties("ObsDir2")="Left"
	Application.UserProperties("PilotDir")="Left"

	Application.UserProperties("Observer1")=""
	Application.UserProperties("Observer2")=""
	Application.UserProperties("Pilot")=""
	
	Application.UserProperties("Aircraft")=""
	Application.UserProperties("Weather")=""
	Application.UserProperties("TransectID")=""
	Application.UserProperties("SegmentID")=1
	Application.UserProperties("AnimalBookmark")=""
	Application.UserProperties("EditAnimalBookmark")=""
	Application.UserProperties("LastAnimalBookmark")=""
	Application.UserProperties("HorizonBookmark")=""
	Application.UserProperties("AnimalID")=1
	Application.UserProperties("HorizonID")=1
	Application.UserProperties("ZoomPer")=50
	Application.UserProperties("Park")=""
	Application.UserProperties("EnableHorizon")	=""
	

	Application.UserProperties("Transect_Mode")="Stop"'Start,Stop
	Application.UserProperties("Current_Mode")=""
	Application.UserProperties("GPS_X")="-1"
	Application.UserProperties("GPS_Y")="-1"
	Application.UserProperties("ThisBK")="-1"
	Application.UserProperties("AnimalName")=""
	Application.UserProperties("CloseForm")="N"
	Application.UserProperties("SegType")=""
	Application.UserProperties("OnTransLength")=0
	Application.UserProperties("TransLength")=0
	Application.UserProperties("ExtendedTransect")=""
	Application.UserProperties("EasyNumber")=""
	Application.UserProperties("Paused")="false"
	Application.UserProperties("PilotSel")="false"
	Application.UserProperties("Obs1Sel")="false"
	Application.UserProperties("Obs2Sel")="false"
	Application.UserProperties("RememberLastSelection")=false
	Application.UserProperties("Year")=GetCurrentYear()


	'sheep form
	Application.UserProperties("CurrentFocusField")=-1
	Application.UserProperties("CurrentFocusFieldValue")=""
	Application.UserProperties("LastSender")=""

	Application.UserProperties("AnimalType")=""
	Application.UserProperties("AnimalForm")=""

	Application.UserProperties("FormOpenMode")="Collect"


	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransectDown").Visible=false
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotRight").Visible=false
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotLeft").Visible=false


	'disable all Transect Buttons when applet starts
	Call DisableTransectButtons
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnLoadMap").Enabled=true
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnLand").Enabled=true
	

end sub

'============================================================================================
'============================================================================================
sub tlbtnLoapMap_Click

     
	dim FullName
	dim ThisFile
	dim ErrorMessage



	'Look for gps point log
	FullName=Application.Path & "\Applets\Survey\GPSPointsLog.shp"
	ErrorMessage=CheckPath(FullName)
	if not ErrorMessage="" then
		msgbox ErrorMessage
		exit sub
	end if

	'Look for  default values file
	FullName=Application.Path & "\Applets\Survey\defaultvalues.dbf"
	ErrorMessage=CheckPath(FullName)
	if not ErrorMessage="" then
		msgbox ErrorMessage
		exit sub
	end if

	'Load the Transect shapefile
	FullName=Application.Path & "\Applets\Survey\TrnOrig.shp"
	ErrorMessage=OpenLayer(FullName)
	if not ErrorMessage ="" then
		msgbox ErrorMessage
		exit sub
	end if



	'set the survey id from the transects layer data
	Application.UserProperties("SurveyID")=GetSurveyID()

	Application.UserProperties("SurveyName")=GetDefaultValue("SurveyName")
	if Application.UserProperties("SurveyName")="" then
		Application.UserProperties("SurveyName")="---"
	end if

	Application.UserProperties("Park")=GetDefaultValue("Park")
	Application.UserProperties("EnableHorizon")=GetDefaultValue("EnableHorizon")
	
	'Load the animal shapefile
	FullName=Application.Path & "\Applets\Survey\animals.shp"
	ErrorMessage=OpenLayer(FullName)
	if not ErrorMessage ="" then
		msgbox ErrorMessage
		exit sub
	end if

	'Check for the TrackLog shapefile
	FullName=Application.Path & "\Applets\Survey\Horizon.shp"
	ErrorMessage= OpenLayer(FullName)
	if not ErrorMessage ="" then
		msgbox ErrorMessage
		exit sub
	end if

	'Check for the TrackLog shapefile
	FullName=Application.Path & "\Applets\Survey\TrackLog.shp"
	ErrorMessage= OpenLayer(FullName)
	if not ErrorMessage ="" then
		msgbox ErrorMessage
		exit sub
	end if


	

	'set map extent to that of the transect layer
	if not Application.Map.Layers("TrnOrig") is nothing then
		Application.Map.Extent = Application.Map.Layers("TrnOrig").Extent
	end if

	'check if the backup point long has any points, if so offer to restore them
	if HasLogFilePoints(Application.Path & "\Applets\PointLog.dbf")=true then
		if msgbox("Backup point data was found in the Backup log file. This indicates that " _
			& " ArcPad may have encountered an error and closed. If this is the case, please  " _
			& " click yes to append these points as a line in the TrackLog",4,"BackupLog")=6 then

			'restore points
			AddBackupLogPointsToTrackLogAsLine Application.Path & "\Applets\PointLog.dbf", _
			Application.Path & "\Applets\Survey\TrackLog.shp"

			'remove and re-add layer for data changes to take effect (map.refresh true doesn't seem to do the trick)
			Map.Layers.Remove("TrackLog.shp")
			FullName=Application.Path & "\Applets\Survey\TrackLog.shp"
			OpenLayer FullName
			
		else


			'get rid of backup points fi user doesn't want to restore them
			DeleteAllBackupLogFilePoints Application.Path & "\Applets\PointLog.dbf"

			'the distance traveled is also stored incase arcmap fails. if the user doesn't want to
			'restore the backup points, then reset the distance to zero
			SetDistanceFromLog Application.Path & _ 
			"\Applets\Survey\defaultvalues.dbf",0

		end if

	end if 


	'remember the full extent and scale
	Application.ExecuteCommand("zoomfullextent")
	set FullExtent = Application.Map.Extent
	FullExtentScale = Application.Map.Scale

	'set the defaultvalues table as the source for picklists
	Call SetPickLists


	'popup the new session form to collect initial values
	Applet.Forms("frmNewSession").Show

end sub

'============================================================================================
'============================================================================================
sub tlbtnCustomZoom_Click

	Dim Scale

	Scale = FullExtentScale * (CDbl(Application.UserProperties("ZoomPer")) / CDbl(100))
	Application.Map.Scale= Scale

end sub


'============================================================================================
'============================================================================================
sub frmEditTransect_Event(SenderName,EventName)

	dim ThisForm,EditFormName,TransectID,AnimalID,AllOk,ErrorMessage,Results,OpenBK
	dim AnimalFormName,SFManager,CurVal,CurTransectID

	set ThisForm=Applet.Forms("frmEditTransect")
	OpenBK=-1

	'===================================================
	if EventName = "onload" then
		ThisForm.Pages("PAGE1").Controls("txtTransectID").Value=""
		ThisForm.Pages("PAGE1").Controls("txtAnimalID").Value=""
	end if

	'===================================================
	if SenderName = "btnEditWeather" then

		AllOk=true
		ErrorMessage=""
		
		'make sure the tramsect os a valid value
		TransectID=ThisForm.Pages("PAGE1").Controls("txtTransectID").Value
		if not IsNumeric(TransectID) then	
			msgbox "Please enter a valid Transect ID"
			AllOk=false
		end if

	
		CurTransectID=Application.UserProperties("TransectID")
		Application.UserProperties("TransectID")=TransectID

		Applet.Forms("frmWeather").Show

		Application.UserProperties("TransectID") = CurTransectID

	end if

	'===================================================
	if SenderName = "btnEditSitings" then
		
		AllOk=true
		ErrorMessage=""
		
		'make sure the tramsect os a valid value
		TransectID=ThisForm.Pages("PAGE1").Controls("txtTransectID").Value
		if not IsNumeric(TransectID) then	
			msgbox "Please enter a valid Transect ID"
			AllOk=false
		end if

		'get the animal id which is optional. only validate on animal id
		'if one was set
		if AllOk=true then
			AnimalID=ThisForm.Pages("PAGE1").Controls("txtAnimalID").Value
			if not IsNumeric(AnimalID) and not Trim(AnimalID)="" then	
				msgbox "Please enter a valid Animal ID or leave field blank"
				AllOk=false
			end if
		end if 



		'get all the animals for the specified transect
		if AllOk=true then

			'returns an animal bk if an animal was specified and could be found
			ErrorMessage=GetTransectAnimalBookmarks(TransectID,AnimalID,OpenBK,AnimalBKPerOffSegment)
			if not ErrorMessage="" then
				msgbox ErrorMessage
				AllOk=false	
			end if

		end if


		'set the first animal record if an animal id was specified
		if AllOk=true then
			if AnimalBKPerOffSegment(0) = "" then
				msgbox "No sightings were found on this transect"
				AllOk=false	
			end if
		end if

		'set the first animal record if an animal id was not specified or could not be found
		if AllOk=true then

			if OpenBK=-1 and Trim(AnimalID)<>"" then
				msgbox "Could not find a sighting with an ID of " & AnimnalID & ". " _
						& "The first sighting on the Transect will be opened instead."
			end if

			if OpenBK=-1 then
				OpenBK=CLng(AnimalBKPerOffSegment(0))
			end if
		end if

		'get the name of the form to use
		if AllOk=true then

			AnimalFormName=GetFeatureValue("animals",OpenBK,"FORMNAME")
			if AnimalFormName="" then
				msgbox "Could not determine which form to use to edit this sighting"
				AllOk=false
			end if

		end if


		'set the animal BK array and load the currently editing animal
		if AllOk=true then

			Application.UserProperties("EditAnimalBookmark")=CStr(OpenBK)

			'tell the form that it is in edit mode and not in collect mode
			Application.UserProperties("FormOpenMode")="Edit"

			Application.UserProperties("Pilot")= _ 
				GetFeatureValueByFilter("TrnOrig","[TransectID]=" & TransectID,"PILOTLNAM")
			Application.UserProperties("Observer1")= _ 
				GetFeatureValueByFilter("TrnOrig","[TransectID]=" & TransectID,"OBSLNAM1")
			Application.UserProperties("Observer2")= _ 
				GetFeatureValueByFilter("TrnOrig","[TransectID]=" & TransectID,"OBSLNAM2")

			CurTransectID=Application.UserProperties("TransectID")
			Application.UserProperties("TransectID")=TransectID



			do while not Application.UserProperties("EditAnimalBookmark")=""



				if Application.UserProperties("AnimalBookmark")<>"" then
					Application.UserProperties("EditAnimalBookmark")= _
						Application.UserProperties("AnimalBookmark")
				end if

				'open the animal form that is associated with the specified sighting record
				Applet.Forms(AnimalFormName).Show

				'get the form name of the current active sighting
				AnimalFormName=GetFeatureValue("animals", _
					Application.UserProperties("AnimalBookmark"),"FORMNAME")



			loop



			'clear out sighting collection
			redim AnimalBKPerOffSegment(0)
			AnimalBKPerOffSegment(0)=""

			Application.UserProperties("FormOpenMode")="Collect"
			Application.UserProperties("AnimalBookmark")=""
			Application.UserProperties("TransectID")=CurTransectID

		end if


	end if

	'===================================================
	if SenderName = "btnAddTransectID" then
		
		CurVal=ThisForm.Pages("PAGE1").Controls("txtTransectID").Value
		CurVal=GetEasyNumber(CurVal)
		if not CurVal="cancel" then
			ThisForm.Pages("PAGE1").Controls("txtTransectID").Value=CurVal
		end if
	end if

	'===================================================
	if SenderName = "btnAddAnimalID" then
		
		CurVal=ThisForm.Pages("PAGE1").Controls("txtAnimalID").Value
		CurVal=GetEasyNumber(CurVal)
		if not CurVal="cancel" then
			ThisForm.Pages("PAGE1").Controls("txtAnimalID").Value=CurVal
		end if
	end if


	'===================================================
	if SenderName = "btnCancel" then
		ThisForm.Close
	end if

	set ThisForm=nothing
	
end sub

'============================================================================================
'============================================================================================
sub tlbtnEditTransect_Click
	Applet.Forms("frmEditTransect").Show
end sub 

'============================================================================================
'============================================================================================
sub tlbtnConfiguration_Click
	Applet.Forms("frmConfiguration").Show
end sub


'============================================================================================
'============================================================================================
sub frmConfiguration_Event(SenderName,EventName)

	Dim ThisForm


	set ThisForm=Applet.Forms("frmConfiguration")

	if EventName="onload" then
			
		if Application.ToolBars("main").Visible=true then
			ThisForm.Pages("PAGE1").Controls("btnHideShowMainToolbar").Text="Hide"
		else
			ThisForm.Pages("PAGE1").Controls("btnHideShowMainToolbar").Text="Show"
		end if

		ThisForm.Pages("PAGE1").Controls("txtZoomPer").value=Application.UserProperties("ZoomPer")
	end if


	if SenderName="btnZoomUp" then
		if CInt(ThisForm.Pages("PAGE1").Controls("txtZoomPer").value)<100 then
			ThisForm.Pages("PAGE1").Controls("txtZoomPer").value= _
				CInt(ThisForm.Pages("PAGE1").Controls("txtZoomPer").value)+10
		end if
	end if

	if SenderName="btnHideShowMainToolbar" then
		

		Application.ToolBars("main").Visible = not Application.ToolBars("main").Visible

		if Application.ToolBars("main").Visible=true then
			ThisForm.Pages("PAGE1").Controls("btnHideShowMainToolbar").Text="Hide"
		else
			ThisForm.Pages("PAGE1").Controls("btnHideShowMainToolbar").Text="Show"
		end if

	end if

	if SenderName="btnZoomDown" then
		if CInt(ThisForm.Pages("PAGE1").Controls("txtZoomPer").value)>10 then
			ThisForm.Pages("PAGE1").Controls("txtZoomPer").value= _
				CInt(ThisForm.Pages("PAGE1").Controls("txtZoomPer").value)-10
		end if
	end if

	if SenderName="btnOK" then

		Application.UserProperties("ZoomPer")=CInt(ThisForm.Pages("PAGE1").Controls("txtZoomPer").value)
		ThisForm.Close
	end if

	if SenderName="btnCancel" then
		ThisForm.Close
	end if

	set ThisForm=nothing

end sub

'============================================================================================
'programmatically set path to defaultvalues.dbf file.
'============================================================================================
sub SetPickLists

	

	'New Session form
	Applet.Forms("frmNewSession").Pages("Page1").Controls("cboPilot").AddItemsFromTable DefaultValuesPath,"PILOT","PILOT"
	Applet.Forms("frmNewSession").Pages("Page1").Controls("cboObserver1").AddItemsFromTable DefaultValuesPath,"OBSERVER","OBSERVER"
	Applet.Forms("frmNewSession").Pages("Page1").Controls("cboObserver2").AddItemsFromTable DefaultValuesPath,"OBSERVER","OBSERVER"
	Applet.Forms("frmNewSession").Pages("Page1").Controls("cboAircraft").AddItemsFromTable DefaultValuesPath,"AIRCRAFT","AIRCRAFT"


	'Mark Animal form
	Applet.Forms("frmMarkAnimal").Pages("Page1").Controls("lstGroupType").AddItemsFromTable DefaultValuesPath,"GROUPTYPE","GROUPTYPE"
	Applet.Forms("frmMarkAnimal").Pages("Page1").Controls("txtPerCover").AddItemsFromTable DefaultValuesPath,"PCTCOVER","PCTCOVER"
	Applet.Forms("frmMarkAnimal").Pages("Page1").Controls("txtSnow").AddItemsFromTable DefaultValuesPath,"PCTSNOW","PCTSNOW"
	Applet.Forms("frmMarkAnimal").Pages("Page1").Controls("lstActivity").AddItemsFromTable DefaultValuesPath,"ACTIVITY","ACTIVITY"

	'Sheep form - short
	Applet.Forms("frmSheepForm").Pages("Page1").Controls("lstActivity").AddItemsFromTable DefaultValuesPath,"ACTIVITY","ACTIVITY"

	'Sheep form - long
	Applet.Forms("frmSheepFormLong").Pages("Page1").Controls("lstActivity").AddItemsFromTable DefaultValuesPath,"ACTIVITY","ACTIVITY"


	'End Transect form
	Applet.Forms("frmEndTransect").Pages("Page1").Controls("cboWeather").AddItemsFromTable DefaultValuesPath,"WEATHER","WEATHER"
	Applet.Forms("frmEndTransect").Pages("Page1").Controls("cboCloudCover").AddItemsFromTable DefaultValuesPath,"CLOUDCOVER","CLOUDCOVER"
	Applet.Forms("frmEndTransect").Pages("Page1").Controls("cboPrecipitation").AddItemsFromTable DefaultValuesPath,"PRECIP","PRECIP"
	Applet.Forms("frmEndTransect").Pages("Page1").Controls("cboTurbulenceIntensity").AddItemsFromTable DefaultValuesPath,"TURBINT","TURBINT"
	Applet.Forms("frmEndTransect").Pages("Page1").Controls("cboTurbulenceDuration").AddItemsFromTable DefaultValuesPath,"TURBDUR","TURBDUR"



end sub

'============================================================================================
'the new session form has been shown and we need to clear all control values and set defaults
'============================================================================================
sub frmNewSession_Load

	Dim ThisList
	Dim ListControl,ThisPage
	Dim ItemCount, ListItem

	

	Set ThisPage = Applet.Forms("frmNewSession").Pages("Page1")

	'reset the controls
	ThisPage.Controls("dtFlownDate").value=""
	ThisPage.Controls("cboObserver1").value=""
	ThisPage.Controls("cboObserver2").value=""
	ThisPage.Controls("cboPilot").value=""
	ThisPage.Controls("cboAircraft").value=""

	'display the survey id
	ThisPage.Controls("lblSurveyIDDisplay").value=Application.UserProperties("SurveyID")
	ThisPage.Controls("lblSurveyName").value=Application.UserProperties("SurveyName")
	Set ThisPage = nothing
	
end sub

'============================================================================================
'if the user cancels the new session form, close the form without doing anything
'============================================================================================
sub frmNewSession_Cancel
	Applet.Forms("frmNewSession").Close
end sub

'============================================================================================
'valid the session values, save them and close the new session form
'============================================================================================
sub frmNewSession_OkClick

	dim ThisForm
	dim Direction
	dim ThisPage



	set ThisForm=Applet.Forms("frmNewSession")
	set ThisPage=ThisForm.Pages("Page1")
	
	'a pilot name is required
	if trim(ThisPage.Controls("cboPilot").value)="" then
		msgbox "Please select the pilot's name"
		exit sub
	end if

	'the first observer name is required
	if trim(ThisPage.Controls("cboObserver1").value)="" then
		msgbox "Please select the first Observer"
		exit sub
	end if


	'the first observer name is required
	if trim(ThisPage.Controls("cboSelAnimal").value)="" then
		msgbox "Please select the name of the animal you will be observing"
		exit sub
	end if

	'set the animal we will be observing
	if LCase(trim(ThisPage.Controls("cboSelAnimal").value))="sheep-short" then
		Application.UserProperties("AnimalType")="SHEEP"
		Application.UserProperties("AnimalForm")="frmSheepForm"
	end if
	if LCase(trim(ThisPage.Controls("cboSelAnimal").value))="sheep-long" then
		Application.UserProperties("AnimalType")="SHEEP"
		Application.UserProperties("AnimalForm")="frmSheepFormLong"
	end if
	if LCase(trim(ThisPage.Controls("cboSelAnimal").value))="bear" then
		Application.UserProperties("AnimalType")=""'will be set from the form
		Application.UserProperties("AnimalForm")="frmMarkAnimal"
	end if

	'store all the selected values
	Application.UserProperties("FlownDate")=ThisPage.Controls("dtFlownDate").value
	Application.UserProperties("Observer1")=ThisPage.Controls("cboObserver1").value
	Application.UserProperties("Observer2")=ThisPage.Controls("cboObserver2").value
	Application.UserProperties("Pilot")=ThisPage.Controls("cboPilot").value
	Application.UserProperties("Aircraft")=ThisPage.Controls("cboAircraft").value

	
	'set up whatever direction is set in the global variable when it was created
	if Application.UserProperties("PilotDir")="Left" then
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotLeft").Visible=true
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotRight").Visible=false
	else
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotLeft").Visible=false
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotRight").Visible=true
	end if
	

	'show only the toolbottons that need to show at this stage after the session info has been set
	Call DisableTransectButtons
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransect").Enabled=true
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnLand").Enabled=true
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransectDown").Enabled=true

	
	ThisForm.Close 

	set ThisForm = nothing
	set ThisPage=nothing

end sub

'============================================================================================
'============================================================================================
sub ObserveDirToolButtons_Event(SenderName,EventName)

	if SenderName="tlbtnPilotLeft" then
		ToggleObserveDirToolbarButtons "Right"
	end if

	if SenderName="tlbtnPilotRight" then
		ToggleObserveDirToolbarButtons "Left"
	end if

end sub

'============================================================================================
'enter select mode to allow user to select a transect
'============================================================================================
sub tlbtnSelectTransect_Click
	


	'open form to allow user to set transect
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransect").Visible=false
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransectDown").Visible=true


	'put transect layer in edit mode so user can make a selection
	'if not Application.Map.Layers("TrnOrig.shp") is nothing then
	'	Application.Map.Layers("TrnOrig.shp").Editable=true
	'end if

		'remove from edit state if previously in edit mode
	'if not Application.Map.Layers("animals.shp") is nothing then
	'	Application.Map.Layers("animals.shp").Editable=false
	'end if

	'put map in select mode
	'Map.PointerMode="modeselect"


	Applet.Forms("frmNewTransect").Show

end sub

'============================================================================================
'exit transect select mode when select transect button is clicked again
'============================================================================================
sub tlbtnSelectTransectDown_Click

	'return transect button from select icon to normal icon
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnStartTransect").Enabled=false
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransect").Visible=true
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransectDown").Visible=false


	SetDisplayValue "Transect ID","---"
	Application.UserProperties("TransectID")=""
	Application.UserProperties("TransLength")=0

end sub

'============================================================================================
'while in transect selection mode, selecting a transect will fire this event so we can determine
'which transect the user selected
'============================================================================================
sub Map_SelectionChange
	
	dim ThisForm,ThisSelLayer,myRS, FlownStatus
	dim SurveyID,TransectID,TrasectPathAndName,AlertMessage

	'if we are not in selection mode, don't do anything
	if Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransectDown").Visible=false then
		exit sub
	end if

	'if the find window is open, when the selection changed, then we tried to select/highlight
	'a found transect. in this case, don't open the form
	if not Applet.Forms("frmFindTransect").Mode=-1 then
		exit sub
	end if


	'make sure a transect has been selected
	set ThisSelLayer = Map.SelectionLayer
	if ThisSelLayer is nothing then
		'Applet.Forms("frmNewTransect").Show
   		exit sub
	end if

	'make sure the layer the selection was made from is the transect layer
	if not ThisSelLayer.Name="TrnOrig.shp" then
		'Applet.Forms("frmNewTransect").Show
		exit sub
	end if



	'point to the selected record in the recordset
	set myRS=ThisSelLayer.Records
	myRS.Bookmark=Map.SelectionBookmark

	'get field info for this record
	if not myRS.Fields("SurveyID") is nothing then
		SurveyID=myRS.Fields("SurveyID").Value
	end if
	if not myRS.Fields("TransectID") is nothing then
		TransectID=myRS.Fields("TransectID").Value
	end if
	if not myRS.Fields("Flown") is nothing then
		FlownStatus=myRS.Fields("Flown").Value
	end if

	Application.UserProperties("TransectID")=TransectID

	AlertMessage=""

	'if the transect is in progress(an error occured) show message
	if FlownStatus="P" then
		AlertMessage="This transect was aborted. Do you still want to select this transect?"
	end if

	'if the transect has already been flown show message
	if FlownStatus="Y" then
		AlertMessage="This transect has already been flown. Would you still like to select it anyway?"
	end if

	if not AlertMessage = "" then
		if msgbox (AlertMessage,4,"Flown Status")=7 then
			exit sub
		end if
	end if
			

	set ThisForm=Applet.Forms("frmNewTransect")
	if not ThisForm is nothing then
		Map.Extent=myRS.Fields.Shape.Extent
		ThisForm.Show
	end if

	set ThisSelLayer =nothing
	set ThisForm=nothing
	set myRS=nothing

end sub

'============================================================================================
'============================================================================================
sub  frmNewTransect_Load

	Dim ThisForm
	Dim ThisPage
	Dim ListTotal,ListCount
	Dim strDBFFile,ThisCB

	
	set ThisForm=Applet.Forms("frmNewTransect")
	set ThisPage=ThisForm.Pages("PAGE1")

	'show the id of 
	ThisPage.Controls("txtTransectID").value=Application.UserProperties("TransectID")
	
		
	'set the first observer
	ThisPage.Controls("lblObserver1").value=Application.UserProperties("Observer1")

	'set the first observer's direction
	if Application.UserProperties("ObsDir1")="Left" then 
		SetControlSelected "frmNewTransect","PAGE1","btnLeft1","btnRight1"
	else
		SetControlSelected "frmNewTransect","PAGE1","btnRight1","btnLeft1"
	end if 

	'set pilot's direction
	if Application.UserProperties("PilotDir")="Left" then 
		SetControlSelected "frmNewTransect","PAGE1","btnPilotLeft","btnPilotRight"
	else
		SetControlSelected "frmNewTransect","PAGE1","btnPilotRight","btnPilotLeft"
	end if 

	'only make second observer options visible if a second observer was selected at the
	'beginning of the session
	if Application.UserProperties("Observer2")="" then

		ThisPage.Controls("btnLeft2").Visible=false
		ThisPage.Controls("btnRight2").Visible=false
		ThisPage.Controls("lblObserver2").Visible=false
	else

		ThisPage.Controls("btnLeft2").Visible=true
		ThisPage.Controls("btnRight2").Visible=true
		ThisPage.Controls("lblObserver2").Visible=true

		ThisPage.Controls("lblObserver2").value=Application.UserProperties("Observer2")

		if Application.UserProperties("ObsDir2")="Left" then 
			SetControlSelected "frmNewTransect","PAGE1","btnLeft2","btnRight2"
		else
			SetControlSelected "frmNewTransect","PAGE1","btnRight2","btnLeft2"
		end if 

	end if

	'set weather option selected from the begining of the session
	'ThisPage.Controls("cbxWeather").value=Application.UserProperties("Weather")
	

	set ThisForm=nothing
	set ThisPage=nothing

end sub

'============================================================================================
'============================================================================================
sub frmNewTransect_CancelClick
	Applet.Forms("frmNewTransect").Close
end sub

'============================================================================================
'============================================================================================
sub frmNewTransect_Event(SenderName,EventName)

	dim ThisPage,CurrentValue,FlownStatus

	set ThisPage=Applet.Forms("frmNewTransect").Pages("Page1")

	if SenderName="btnPilotLeft" then
		SetObservingDirectionForAll "Left"
	end if

	if SenderName="btnLeft1" then
		SetObservingDirectionForAll "Left"
	end if

	if SenderName="btnEnterTransectID" then
		CurrentValue=ThisPage.Controls("txtTransectID").Value
		CurrentValue=GetEasyNumber(CurrentValue)
		if not CurrentValue="cancel" then
			ThisPage.Controls("txtTransectID").Value=CurrentValue
		end if
	end if

	if SenderName="btnLeft2" then
		SetObservingDirectionForAll "Left"
	end if
	
	if SenderName="btnPilotRight" then
		SetObservingDirectionForAll "Right"
	end if

	if SenderName="btnRight1" then
		SetObservingDirectionForAll "Right"
	end if

	if SenderName="btnRight2" then
		SetObservingDirectionForAll "Right"
	end if

	set ThisPage=nothing

end sub

'============================================================================================
'============================================================================================
sub frmNewTransect_OkClick

	Dim ThisForm
	Dim ThisPage
	Dim ListTotal,ListCount
	Dim strDBFFile,lyThisLayer
	Dim myRS,TransectID,ErrorMessage,AlertMessage,FlownStatus






	set ThisForm=Applet.Forms("frmNewTransect")
	set ThisPage=ThisForm.Pages("PAGE1")


	TransectID=ThisPage.Controls("txtTransectID").value
	if trim(TransectID)="" or IsNumeric(TransectID)=false  then
		msgbox "Please enter a valid TransectID"
	exit sub
	end if


	'make sure the transect id is a valid transect
	ErrorMessage=IsATransect(TransectID)
	if ErrorMessage<>"" then
		msgbox ErrorMessage
		exit sub
	end if

	FlownStatus=GetTransectFlownStatus_2(Application.UserProperties("SurveyID"), _
		TransectID)

	'if the transect is in progress(an error occured) show message
	if FlownStatus="P" then
		AlertMessage="This transect was aborted. Do you still want to select this transect?"
	end if

	'if the transect has already been flown show message
	if FlownStatus="Y" then
		AlertMessage="This transect has already been flown. Would you still like to select it anyway?"
	end if

	if not AlertMessage = "" then
		if msgbox (AlertMessage,4,"Flown Status")=7 then
			exit sub
		end if
	end if

	'zoom to the selected transect
	ZoomToTransect TransectID


	'save direction
	if  GetControlSelectedState("frmNewTransect","PAGE1","btnPilotLeft")=true then
		Application.UserProperties("PilotDir")="Left"
		Application.UserProperties("ObsDir1")="Left"
		if not Application.UserProperties("Observer2")="" then
			Application.UserProperties("ObsDir2")="Left"
		end if
	else
		Application.UserProperties("PilotDir")="Right"
		Application.UserProperties("ObsDir1")="Right"
		if not Application.UserProperties("Observer2")="" then
			Application.UserProperties("ObsDir2")="Right"
		end if
	end if

	'change direction icon
	if Application.UserProperties("PilotDir")="Left" then
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotLeft").Visible=true
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotRight").Visible=false
	else
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotLeft").Visible=false
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotRight").Visible=true
	end if

	
	SetDisplayValue "Transect ID",TransectID
	Application.UserProperties("TransectID")=TransectID

	'go to transect record
	set lyThisLayer=Application.Map.Layers("TrnOrig")
	set myRS=lyThisLayer.Records
	myRS.Bookmark=myRS.Find("[TransectID]=" & TransectID) 


	'get the information we need off of the selected transect
	if not myRS.Fields("TransectID") is nothing then
		Application.UserProperties("TransectID")=myRS.Fields("TransectID").Value
	end if
	'TODO:  get length from custom length field
	if not myRS.Fields("Shape_Leng") is nothing then
		Application.UserProperties("TransLength")=myRS.Fields("TARGETLEN").Value
	end if
 	
	
	Call DisableTransectButtons
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransect").Enabled=true
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransectDown").Enabled=true
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnStartTransect").Enabled=true
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnLand").Enabled=true


	ThisForm.Close

	set lyThisLayer=nothing
	set myRS=nothing
	set ThisForm=nothing
	set ThisPage=nothing

end sub

'============================================================================================
'============================================================================================
sub tlbtnStartTransect_Click

	dim myRS,TransectWithSegs,lyThisLayer
	Dim SurveyID,TransectID,TrasectPathAndName,LastSegmentID,SegmentID
	dim TrackLogPathAndName,SegTypeOnArcPadFail,AnimalID
	dim GPSPoint,FlownStatus,TransLength
	dim ArcMapErrorOccured,AlertMessage,Distance



	TransectID=Application.UserProperties("TransectID")


	'make sure a feature is selected
	if trim(TransectID)="" then
		msgbox "No Transect has been set. To set a Transect, click the 'Select Transect' " & _
		" button and fill out the New Transect form. "
		exit sub
	end if

	SurveyID=Application.UserProperties("SurveyID")
	TrasectPathAndName=Application.Path & "\Applets\Survey\TrnOrig.shp"
	TrackLogPathAndName=Application.Path & "\Applets\Survey\TrackLog.shp"

	'we may be reflying a transect which has animal data already so we need to find the highest
	'animal id and continue marking new sightings from that value upwards
	AnimalID=GetLastAnimalIDForTransect(TransectID,SurveyID)
	if AnimalID>-1 then
		AnimalID=AnimalID+1
		Application.UserProperties("AnimalID")=AnimalID
	end if
	

	
	'make sure the gps is open
	if GPS.IsOpen=false then
		msgbox "No active GPS port was found. Transects cannot be flown without an active GPS port"
		exit sub
	end if


	'clear extended transect flag used to determine when a transect is been flown for a distance longer
	'than it really is
	Application.UserProperties("ExtendedTransect")=""

	TransLength=BackUpPointsTransLengthSoFar(Application.Path & "\Applets\Survey\TrackLog.shp")
	if TransLength<0 then
	end if
	
	'get the gps point for the current position
	set GPSPoint=Application.CreateAppObject ("Point")
	GPSPoint.X=GPS.X
	GPSPoint.Y=GPS.Y
	GPSPoint.Z=GPS.Z

	'reset ourl sighting array
	redim AnimalBKPerOffSegment(0)
	AnimalBKPerOffSegment(0)=""

	set lyThisLayer=Application.Map.Layers("TrnOrig")
	lyThisLayer.Editable=true

	set myRS=lyThisLayer.Records
	myRS.Bookmark=myRS.Find("[TransectID]=" & TransectID) 

	'get selected transect's flown status
	FlownStatus=myRS.Fields("Flown").Value
		
	'set the selected transect's flown status to in progress
	myRS.Fields("Flown").Value="P"

	'set all the default values we have collected thus far
	if not myRS.Fields("WEATHER") is nothing then
		IF not Application.UserProperties("Weather")="" then
			myRS.Fields("WEATHER").Value=Application.UserProperties("Weather")
		end if
	end if
	if not myRS.Fields("Aircraft") is nothing then
		IF not Application.UserProperties("Aircraft")="" then
			myRS.Fields("Aircraft").Value=Application.UserProperties("Aircraft")
		end if
	end if
	if not myRS.Fields("PILOTLNAM") is nothing then
		IF not Application.UserProperties("Pilot")="" then
			myRS.Fields("PILOTLNAM").Value=Application.UserProperties("Pilot")
		end if
	end if
	if not myRS.Fields("OBSLNAM1") is nothing then
		IF not Application.UserProperties("Observer1")="" then
			myRS.Fields("OBSLNAM1").Value=Application.UserProperties("Observer1")
		end if
	end if
	if not myRS.Fields("OBSLNAM2") is nothing then
		IF not Application.UserProperties("Observer2")="" then
			myRS.Fields("OBSLNAM2").Value=Application.UserProperties("Observer2")
		end if
	end if
	if not myRS.Fields("FLOWNDATE") is nothing then

		if not Application.UserProperties("FlownDate")="" then

			Dim ThisDate,ThisDateObject,ShortYear,ShortDate
			ThisDate=Application.UserProperties("FlownDate")
				
			if IsDate(ThisDate) then

				ThisDateObject=CDate(ThisDate)
				ShortYear=Year(ThisDateObject)
				ShortYear=Mid(ShortYear,3,4)

				ShortDate=Month(ThisDateObject) & "/" & _ 
				Day(ThisDateObject) & "/" & ShortYear

				myRS.Fields("FLOWNDATE").Value=ShortDate
			end if
				
		end if

	end if

	myRS.Update

	lyThisLayer.Editable=false

	set myRS=nothing
	set lyThisLayer=nothing




	'new transect, no log data,start from segment 1
	if FlownStatus ="N" then

		'if this is a new transect, start from 1 (0 will increment before being added to transect
		Application.UserProperties("SegmentID")=1

		'remove any backup points that may still be in the log
		DeleteAllBackupLogFilePoints Application.Path & "\Applets\PointLog.dbf"
	
		'add the exact point at which user clicked start transect as first point in log
		AddPointToBackupLog Application.Path & "\Applets\PointLog.dbf",GPSPoint.X, _ 
		GPSPoint.Y,GPSPoint.Z,SurveyID,TransectID,SegmentID,"OnTransect"

		'add the exact point at which user clicked start transect as first point in point collection
		RestartGlobalCollectionWithCurrentGPSPoint GPSPoint.X, _ 
		GPSPoint.Y,GPSPoint.Z

		'set the starting transect distance in the log as 0
		SetDistanceFromLog Application.Path & "\Applets\Survey\defaultvalues.dbf",0

		'set the starting transect distance in the transect length variable as 0
		Application.UserProperties("OnTransLength")=0
		SetDisplayValue "Distance Traveled","0.000 km"

		'tell gps to start collection points and tell log that those points are ontransect points
		Application.UserProperties("SegType")="OnTransect"
		SetDisplayValue "Transect Status","On"
		Application.UserProperties("Transect_Mode")="Start"


		Call DisableTransectButtons
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnEndTransect").Enabled=true
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnMarkAnimal").Enabled=true
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnMarkHorizon").Enabled=true
	
  	
	end if
	
	'possible arcpad fatal error left transect in progress state
	if FlownStatus="P" then

			'get the last segmentid to continue from
			SegmentID=GetHighestSegmentID (Application.Path & _ 
					"\Applets\Survey\TrackLog.shp",SurveyID,TransectID)

			'if there were not other segments, we will start with 1
			if SegmentID=-1 then
				SegmentID=1
				Application.UserProperties("SegmentID")=SegmentID
			else
				Application.UserProperties("SegmentID")=SegmentID
			end if

			'see if there are any old gps backuplog points. the log will let us know
			'if they were on or off transect when the error occured
			ArcMapErrorOccured=HasRecords(Application.Path & "\Applets\PointLog.dbf")

			if ArcMapErrorOccured=true then

				'if an error occured, the log distance traveled will have the distance so far
				'we'll start measuring distance from there.else, we'll start from zero
				Distance=GetDistanceFromLog(Application.Path & "\Applets\Survey\defaultvalues.dbf")
				if not IsNumeric(Distance) then
						
					'set starting distance in log to 0
					SetDistanceFromLog Application.Path & "\Applets\Survey\defaultvalues.dbf",0

					Distance=0
				end if

			else

				'set starting distance in log to 0
				SetDistanceFromLog Application.Path & _ 
				"\Applets\Survey\defaultvalues.dbf",0

				Distance=0
			end if
		


			'set starting transect distance
			Application.UserProperties("OnTransLength")=CDbl(Distance)
			'Application.Toolbars("tlbNPSDisplay").Caption="Distance Traveled:" _ 
			'& Round((CDbl(Distance)/1000),3) & " km"

			SetDisplayValue "Distance Traveled",Round((CDbl(Distance)/1000),3) & " km"



			if ArcMapErrorOccured=true then

				'if points exist, SegType will have either ontransect or offtransect as a value
				SegTypeOnArcPadFail=GetGPSPointCollectionFieldValue( _ 
						Application.Path & "\Applets\PointLog.dbf","SegType")


				
				Call DisableTransectButtons
				Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnEndTransect").Enabled=true
				Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnMarkAnimal").Enabled=true
				Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnMarkHorizon").Enabled=true

				'remove all backup points
				DeleteAllBackupLogFilePoints Application.Path & "\Applets\PointLog.dbf"

				'set mode to offtransect and open animal form
				if SegTypeOnArcPadFail="OffTransect" then

					'add the exact point at which user clicked start transect as first point in log
					AddPointToBackupLog Application.Path & "\Applets\PointLog.dbf",GPSPoint.X, _ 
						GPSPoint.Y,GPSPoint.Z,SurveyID,TransectID,SegmentID,"OffTransect"

					'add the exact point at which user clicked start transect as first point in point collection
					RestartGlobalCollectionWithCurrentGPSPoint GPSPoint.X, _ 
						GPSPoint.Y,GPSPoint.Z

					'tell gps to start collection points and tell log that those points are offtransect points
					Application.UserProperties("SegType")="OffTransect"
					SetDisplayValue "Transect Status","Off"
					Application.UserProperties("Transect_Mode")="Start"

					
					redim AnimalBKPerOffSegment(0)
					AnimalBKPerOffSegment(0)=""

					ResetAnimalBookmarkArrayToSegementsAnimals SegmentID,TransectID,SurveyID
	

					Applet.Forms(Application.UserProperties("AnimalForm")).Show


				end if

				'start sample ontransect
				if SegTypeOnArcPadFail="OnTransect" or  SegTypeOnArcPadFail="" then

			

					'add the exact point at which user clicked start transect as first point in log
					AddPointToBackupLog Application.Path & "\Applets\PointLog.dbf",GPSPoint.X, _ 
						GPSPoint.Y,GPSPoint.Z,SurveyID,TransectID,SegmentID,"OnTransect"

					'add the exact point at which user clicked start transect as first point in point collection
					RestartGlobalCollectionWithCurrentGPSPoint GPSPoint.X, _ 
						GPSPoint.Y,GPSPoint.Z

					'tell gps to start collection points and tell log that those points are ontransect points
					Application.UserProperties("SegType")="OnTransect"
					SetDisplayValue "Transect Status","On"
					Application.UserProperties("Transect_Mode")="Start"


				end if

				
			else

				'add the exact point at which user clicked start transect as first point in log
				AddPointToBackupLog Application.Path & "\Applets\Log.dbf",GPSPoint.X, _ 
				GPSPoint.Y,GPSPoint.Z,SurveyID,TransectID,SegmentID,"OnTransect"


				'add the exact point at which user clicked start transect as first point in point collection
				RestartGlobalCollectionWithCurrentGPSPoint GPSPoint.X, _ 
				GPSPoint.Y,GPSPoint.Z

				'tell gps to start collection points and tell log that those points are ontransect points
				Application.UserProperties("SegType")="OnTransect"
				SetDisplayValue "Transect Status","On"
				Application.UserProperties("Transect_Mode")="Start"

				Call DisableTransectButtons
				Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnEndTransect").Enabled=true
				Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnMarkAnimal").Enabled=true
				Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnMarkHorizon").Enabled=true

			end if

	end if

	'user is reflying a transect. start in ontransect mode but from the last segment id
	if FlownStatus="Y" then
		
			'if user is reflying transect, start segment from last segmentid
			SegmentID=GetHighestSegmentID (Application.Path & _ 
					"\Applets\Survey\TrackLog.shp",SurveyID,TransectID)

			if SegmentID=-1 then
				SegmentID=1
				Application.UserProperties("SegmentID")=SegmentID
			else
				Application.UserProperties("SegmentID")=SegmentID
			end if

			'set starting transect distance
			SetDistanceFromLog Application.Path & _ 
			"\Applets\Survey\defaultvalues.dbf",0
			Application.UserProperties("OnTransLength")=0
			'Application.Toolbars("tlbNPSDisplay").Caption="Distance Traveled:0 km"

			SetDisplayValue "Distance Traveled","0.000 km"

			'remove all backup points
			DeleteAllBackupLogFilePoints Application.Path & "\Applets\PointLog.dbf"

			
			'add the exact point at which user clicked start transect as first point in log
			AddPointToBackupLog Application.Path & "\Applets\PointLog.dbf",GPSPoint.X, _ 
			GPSPoint.Y,GPSPoint.Z,SurveyID,TransectID,SegmentID,"OnTransect"

			'add the exact point at which user clicked start transect as first point in point collection
			RestartGlobalCollectionWithCurrentGPSPoint GPSPoint.X, _ 
			GPSPoint.Y,GPSPoint.Z

			'tell gps to start collection points and tell log that those points are ontransect points	
			Application.UserProperties("SegType")="OnTransect"
			SetDisplayValue "Transect Status","On"
			Application.UserProperties("Transect_Mode")="Start"
			

			
			Call DisableTransectButtons
			Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnEndTransect").Enabled=true
			Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnMarkAnimal").Enabled=true
			Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnMarkHorizon").Enabled=true

	end if


	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransect").Enabled=false
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransectDown").Enabled=false
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransect").Visible=true
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransectDown").Visible=false
	Map.PointerMode="modenone"

	'display transect id
	SetDisplayValue "Transect ID",CStr(TransectID)

	set GPSPoint=nothing
	set myRS=nothing

end sub


'============================================================================================
'============================================================================================
sub tlbtnMarkAnimal_Click

	
	Dim ErrorMessage,SegmentID,SurveyID,TransectID
	Dim GPSPoint

	
	'ErrorMessage=CheckGPS()
	'if not ErrorMessage="" then
	'	msgbox ErrorMessage
	'	exit sub
	'end if

	'get our current globals
	TransectID=Application.UserProperties("TransectID")
	SurveyID=Application.UserProperties("SurveyID")
	SegmentID=Application.UserProperties("SegmentID")
	
	'if the user is clicking the mark animal button to reopen the form because it was hiden
	'then we simple restore the form
	if Application.UserProperties("Paused")="true" then

		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnEndTransect").Visible=true
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotRight").Enabled=true
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotLeft").Enabled=true
		Applet.Forms(Application.UserProperties("AnimalForm")).Show
		exit sub
	end if

	

	'make sure that if the gps is still firing, new points will not be added to the on-transect point collection
	Application.UserProperties("Transect_Mode")="Pause"

	'collection all the points thus far including the current location and 
	'create the on-transect segment before we go off transect
	set GPSPoint=Application.CreateAppObject ("Point")
	GPSPoint.X=GPS.X
	GPSPoint.Y=GPS.Y
	GPSPoint.Z=GPS.Z
	ErrorMessage=MakeLineFromGPSPointsAndAddToLayer(GPSPoint,"TrackLog", _ 
		SurveyID,TransectID,CStr(SegmentID),"OnTransect")

	
	'we are now on an off transect segment so flag our global property as such
	Application.UserProperties("SegType")="OffTransect"

	'update the display to show that we are now off transect
	SetDisplayValue "Transect Status","Off"

	'delete the backup points for the previous segment since the segment was completed successfully
	DeleteAllBackupLogFilePoints Application.Path & "\Applets\PointLog.dbf"

	'start collecting backup points for the off transect segment begining with the current location
	AddPointToBackupLog Application.Path & "\Applets\PointLog.dbf",GPSPoint.X, _ 
		GPSPoint.Y,GPSPoint.Z,SurveyID,TransectID,SegmentID,"OffTransect"

	'clear and restart our segment point collection to start collection points which will later be
	'used to build the current off-transect segment
	RestartGlobalCollectionWithCurrentGPSPoint GPSPoint.X, _ 
		GPSPoint.Y,GPSPoint.Z

	'tell gps that it can now start collecting points again and adding them to our current segment
	'point collection
	Application.UserProperties("Transect_Mode")="Start"

	
	'show the current animal form we are using to collection sighting data
	Applet.Forms(Application.UserProperties("AnimalForm")).Show

	'disable all buttons and enable the only two buttons available during a transect flight,
	'mark animal and end transect
	Call DisableTransectButtons
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnMarkAnimal").Enabled=true
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnEndTransect").Enabled=true

	set GPSPoint=nothing



end sub

'============================================================================================
'Clear all field data to prepare for another animal sighting
'============================================================================================
sub RefreshMarkAnimalForm

	dim ThisForm
	dim ThisPage,Current_Value
	dim ListControl

	set ThisForm=Applet.Forms("frmMarkAnimal")
		

	set ListControl=Applet.Forms("frmMarkAnimal").Pages("PAGE1").Controls("lstActivity")
	PopulateComboBox DefaultValuesPath,ListControl,"ACTIVITY"

	set ListControl=Applet.Forms("frmMarkAnimal").Pages("PAGE1").Controls("lstAnimalList")
	PopulateComboBox DefaultValuesPath,ListControl,"ANIMAL"

	set ListControl=Applet.Forms("frmMarkAnimal").Pages("PAGE1").Controls("lstGroupType")
	PopulateComboBox DefaultValuesPath,ListControl,"GROUPTYPE"

	ThisForm.Pages("PAGE1").Controls("txtSnow").ListIndex=-1
	ThisForm.Pages("PAGE1").Controls("txtGroupSize").Text="0"
	ThisForm.Pages("PAGE1").Controls("txtPerCover").ListIndex=-1

	ThisForm.Pages("PAGE1").Controls("txtPilotRept").Text=""
	ThisForm.Pages("PAGE1").Controls("txtObsRept").Text=""

	SetControlNotSelected "frmMarkAnimal","PAGE1","btnPilotSawIt"
	SetControlNotSelected "frmMarkAnimal","PAGE1","btnObsSawIt"
	SetControlNotSelected "frmMarkAnimal","PAGE1","btnBothSawIt"

	ThisForm.Pages("PAGE1").Controls("txtAnimalID").Text=""


	set ThisForm=nothing
	set ThisPage=nothing
	set ListControl=nothing

end sub

'============================================================================================
'============================================================================================
sub frmMarkAnimal_Load
	
	dim ThisForm
	dim ThisPage,Current_Value
	dim NextAnimalBK


	set ThisForm=Applet.Forms("frmMarkAnimal")


	Application.UserProperties("AnimalBookmark")=""
	call RefreshMarkAnimalForm
	
	'reopen to last editing sighting after a crash that occured offsegment
	if not AnimalBKPerOffSegment(0)="" and not Application.UserProperties("FormOpenMode")="Edit" then

		NextAnimalBK=AnimalBKPerOffSegment(UBound(AnimalBKPerOffSegment))
		Application.UserProperties("AnimalBookmark")=NextAnimalBK
		'Call RefreshMarkAnimalForm
		LoadAnimal NextAnimalBK

	end if

	SetFormEditMode "frmMarkAnimal",true

	'load the sighting to edit if we are in edit mode
	if Application.UserProperties("FormOpenMode")="Edit" then
		
		
		SetFormEditMode "frmMarkAnimal",false

		Application.UserProperties("AnimalBookmark")= _
			Application.UserProperties("EditAnimalBookmark")
		LoadAnimal Application.UserProperties("AnimalBookmark")

	end if

	ThisForm.Pages("PAGE1").Controls("txtTransectID").Value=Application.UserProperties("TransectID")

	set ThisForm=nothing
	set ThisPage=nothing



end sub

'============================================================================================
'============================================================================================
sub SetFormEditMode(FormName,IsCollectMode)

	dim ThisForm,IsEditMode

	set ThisForm=Applet.Forms(FormName)

	ThisForm.Pages("PAGE1").Controls("btnMarkAnimal").Visible=IsCollectMode
	ThisForm.Pages("PAGE1").Controls("btnUpdatePosition").Visible=IsCollectMode
	ThisForm.Pages("PAGE1").Controls("btnUpdatePosition").Visible=IsCollectMode

	ThisForm.Pages("PAGE1").Controls("btnMarkHorizon").Visible=IsCollectMode
	ThisForm.Pages("PAGE1").Controls("btnUpdateHorizon").Visible=IsCollectMode
	ThisForm.Pages("PAGE1").Controls("btnUseLastHorizon").Visible=IsCollectMode

	'ThisForm.Pages("PAGE1").Controls("btnPause").Visible=IsCollectMode
	ThisForm.Pages("PAGE1").Controls("btnResumeTransect").Visible=IsCollectMode

	if IsCollectMode=false then 
		IsEditMode=true 
	else 
		IsEditMode=false 
	end if

	ThisForm.Pages("PAGE1").Controls("btnCloseEdit").Visible=IsEditMode

	if FormName="frmMarkAnimal" then


	end if

	set ThisForm=nothing

end sub

'============================================================================================
'============================================================================================
sub frmMarkAnimal_Event(Sender,EventName)

	Dim ThisForm


	set ThisForm=Applet.Forms("frmMarkAnimal")


	'=============================================================
	'INACTIVE - button was removed on client request
	'=============================================================
	if Sender="btnReloadLast" then

		dim AllOK,PrevAnimalBK,CurAnimalID
		AllOK=true

		
		if AnimalBKPerOffSegment(0)="" then
			msgbox "There are no previous animal sightings."
			AllOK=false
		end if

		if AllOK=true then
			if UBound(AnimalBKPerOffSegment) < 1 then
				AllOK=false
			end if
		end if


		if AllOK=true then
			CurAnimalID=Applet.Forms("frmMarkAnimal").Pages("PAGE1").Controls("txtAnimalID").Value
			PrevAnimalBK=AnimalBKPerOffSegment(UBound(AnimalBKPerOffSegment)-1)
			LoadAnimal PrevAnimalBK
			Applet.Forms("frmMarkAnimal").Pages("PAGE1").Controls("txtAnimalID").Value=CurAnimalID
		end if
		
		
	end if

	'=============================================================
	'=============================================================
	if Sender="btnPilotSawIt" then

		if GetControlSelectedState("frmMarkAnimal","PAGE1","btnPilotSawIt")=false then

			SetControlSelected "frmMarkAnimal","PAGE1","btnPilotSawIt",""
			SetControlNotSelected "frmMarkAnimal","PAGE1","btnObsSawIt"
			SetControlNotSelected "frmMarkAnimal","PAGE1","btnBothSawIt"
			ThisForm.Pages("PAGE1").Controls("txtObsRept").value=""
		end if

	end if

	'=============================================================
	'=============================================================
	if Sender="btnObsSawIt" then

		if GetControlSelectedState("frmMarkAnimal","PAGE1","btnObsSawIt")=false then

			SetControlSelected "frmMarkAnimal","PAGE1","btnObsSawIt",""
			SetControlNotSelected "frmMarkAnimal","PAGE1","btnPilotSawIt"
			SetControlNotSelected "frmMarkAnimal","PAGE1","btnBothSawIt"
			ThisForm.Pages("PAGE1").Controls("txtPilotRept").value=""
		end if

	end if

	'=============================================================
	'=============================================================
	if Sender="btnBothSawIt" then

		if GetControlSelectedState("frmMarkAnimal","PAGE1","btnBothSawIt")=false then

			SetControlSelected "frmMarkAnimal","PAGE1","btnBothSawIt",""
			SetControlNotSelected "frmMarkAnimal","PAGE1","btnObsSawIt"
			SetControlNotSelected "frmMarkAnimal","PAGE1","btnPilotSawIt"
	
		end if

	end if

	'=============================================================
	'=============================================================
	if Sender="btnCloseEdit" then
		UpdateMarkAnimal Application.UserProperties("AnimalBookmark")
		Application.UserProperties("EditAnimalBookmark")=""
		ThisForm.Close
	end if


	set ThisForm=nothing


end sub

'============================================================================================
'============================================================================================
sub btnMarkAnimal_Click

	dim lyrAnimal,lyrTrnOrig
	dim ErrorMessage
	dim myRS,TransectID,SegmentID,SurveyID
	dim Species,GroupSize,GroupType,Activity,WhoSawIt,Cover,Snow
	dim ThisForm,Latitude,Longitude
	DIM X,Y,Z,Altitude,ThisDate,ViewSide
	dim PilotLastName,PilotRep,ObsRep,ObsLName,ThisPoint,AnimalID
	dim PilotSawIt,Obs1SawIt,Obs2SawIt,Obs1Rep,Obs2Rep

	
	ErrorMessage=CheckGPS()
	if not ErrorMessage="" then
		msgbox ErrorMessage
		exit sub
	end if

	set ThisForm=Applet.Forms("frmMarkAnimal")

	'if we have an animalbookmark set, then we are updating
	if not Application.UserProperties("AnimalBookmark")="" then
		UpdateMarkAnimal Application.UserProperties("AnimalBookmark")
		Application.UserProperties("LastAnimalBookmark")=Application.UserProperties("AnimalBookmark")
		'if Application.UserProperties("RememberLastSelection")=false then
		Call RefreshMarkAnimalForm
		'end if
	end if


	
	
	'make the TrnOrig editable false
	'set lyrTrnOrig = Application.Map.Layers("TrnOrig")
	'if not lyrTrnOrig is nothing then
		'lyrTrnOrig.Editable=false
	'end if
	call RemoveAllLayersFromEditMode

	'make sure the layer is in the map
	set lyrAnimal = Application.Map.Layers("animals")
	if lyrAnimal is nothing then
		msgbox "Could not find the Animal layer"
		exit sub
	end if

	'make the layer editable to do select
	if lyrAnimal.CanEdit=true then 
		lyrAnimal.Editable=true
	else	
		msgbox "not editable"
		exit sub
	end if


	AnimalID=Application.UserProperties("AnimalID")


	'create a point
	set ThisPoint=Application.CreateAppObject ("Point")
	ThisPoint.X=GPS.X
	ThisPoint.Y=GPS.Y
	ThisPoint.Z=GPS.Z
	Altitude=GPS.Altitude
	Latitude=GPS.Latitude
	Longitude=GPS.Longitude

	set myRS=lyrAnimal.Records
	myRS.AddNew ThisPoint


	'here we keep track of every new record created by saving its bookmark in an array
	'this will be used later in the the prev/next buttons to move between animal sightings per
	'off-transect segment
	Application.UserProperties("AnimalBookmark")=CStr(myRS.Bookmark)
	if AnimalBKPerOffSegment(0)="" then
		AnimalBKPerOffSegment(UBound(AnimalBKPerOffSegment))=CStr(myRS.Bookmark)
	else
		redim preserve AnimalBKPerOffSegment(UBound(AnimalBKPerOffSegment)+1)
		AnimalBKPerOffSegment(UBound(AnimalBKPerOffSegment))=CStr(myRS.Bookmark)
	end if



	SegmentID=CLng(Application.UserProperties("SegmentID"))
	TransectID=Application.UserProperties("TransectID")
	SurveyID=Application.UserProperties("SurveyID")



	if not myRS.Fields("SurveyName") is nothing then
		myRS.Fields("SurveyName").Value=Application.UserProperties("SurveyName")
	end if
	if not myRS.Fields("FORMNAME") is nothing then
		myRS.Fields("FORMNAME").Value=Application.UserProperties("AnimalForm")
	end if
	if not myRS.Fields("Park") is nothing then
		myRS.Fields("Park").Value=Application.UserProperties("Park")
	end if
	if not myRS.Fields("Year") is nothing then
		myRS.Fields("Year").Value=GetCurrentYear()
	end if
	if not myRS.Fields("TransectID") is nothing then
		myRS.Fields("TransectID").Value=TransectID
	end if
	if not myRS.Fields("SegmentID") is nothing then
		myRS.Fields("SegmentID").Value=SegmentID
	end if
	if not myRS.Fields("SurveyID") is nothing then
		myRS.Fields("SurveyID").Value=SurveyID
	end if

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
	if not myRS.Fields("AnimalID") is nothing then
		myRS.Fields("AnimalID").Value=AnimalID
		ThisForm.Pages("PAGE1").Controls("txtAnimalID").Value=CStr(AnimalID)
	end if

	if not myRS.Fields("DIST2TRANS") is nothing then
		myRS.Fields("DIST2TRANS").Value=DistanceFromLastSegment(ThisPoint)
	end if
	

	myRS.Update
	

	lyrAnimal.Editable=false
	UpdateMarkAnimal Application.UserProperties("AnimalBookmark")
	AnimalID=AnimalID+1
	Application.UserProperties("AnimalID")=AnimalID

	set myRS=nothing
	set lyrAnimal=nothing
	set ThisPoint=nothing
	set ThisForm=nothing
	set lyrTrnOrig=nothing

end sub

'============================================================================================
'============================================================================================
sub btnPrevAnimal_Clicked

	dim CurrentAnimalBK,PrevAnimalBK
	dim Index

	if AnimalBKPerOffSegment(0)="" then
		msgbox "There are no animal sightings to navigate through."
		exit sub
	end if

	CurrentAnimalBK=Application.UserProperties("AnimalBookmark")

	UpdateMarkAnimal CurrentAnimalBK
	Index=FindIndexByText(AnimalBKPerOffSegment,CurrentAnimalBK)
	if Index > 0  then
		PrevAnimalBK=AnimalBKPerOffSegment(Index-1)
		Call RefreshMarkAnimalForm
		LoadAnimal PrevAnimalBK


		Application.UserProperties("AnimalBookmark")=PrevAnimalBK
	end if

	
end sub

'============================================================================================
'============================================================================================
sub btnNextAnimal_Clicked

	dim CurrentAnimalBK,NextAnimalBK
	dim Index

	if AnimalBKPerOffSegment(0)="" then
		msgbox "There are no animal sightings to navigate through."
		exit sub
	end if

	CurrentAnimalBK=Application.UserProperties("AnimalBookmark")

	UpdateMarkAnimal CurrentAnimalBK
	Index=FindIndexByText(AnimalBKPerOffSegment,CurrentAnimalBK)

	if Index < UBound(AnimalBKPerOffSegment)  then
		NextAnimalBK=AnimalBKPerOffSegment(Index+1)
		Call RefreshMarkAnimalForm
		LoadAnimal NextAnimalBK


		Application.UserProperties("AnimalBookmark")=NextAnimalBK
	end if

end sub


'============================================================================================
'============================================================================================
sub LoadAnimal(AnimalBookmark)

		dim lyrAnimal,Bookmark,myRS,ThisForm
		Dim DidPilotSeeIt,DidObsSeeIt

		'make sure the layer is in the map
		set lyrAnimal = Application.Map.Layers("animals")
		if lyrAnimal is nothing then
			msgbox "Could not find the Animal layer"
			exit sub
		end if

		set myRS=lyrAnimal.Records
		Bookmark=CLng(AnimalBookmark)
		myRS.Bookmark=Bookmark

		set ThisForm=Applet.Forms("frmMarkAnimal")

		Call RefreshMarkAnimalForm	
		

		DidPilotSeeIt=false
		DidObsSeeIt=false

		if not myRS.Fields("PILOTLNAM") is nothing then
			if not myRS.Fields("PILOTLNAM").IsNull and not myRS.Fields("PILOTLNAM")=""  then

				DidPilotSeeIt=true

				if not myRS.Fields("PILOTREPT") is nothing then
					if not myRS.Fields("PILOTREPT").IsNull and not myRS.Fields("PILOTREPT")=-1 then
						ThisForm.Pages("PAGE1").Controls("txtPilotRept").Value=CStr(myRS.Fields("PILOTREPT"))
					else
						ThisForm.Pages("PAGE1").Controls("txtPilotRept").Value=""
					end if
				end if

			end if
		end if



		if not myRS.Fields("OBS1LNAM") is nothing then
			if not myRS.Fields("OBS1LNAM").IsNull  and not myRS.Fields("OBS1LNAM")="" then

				DidObsSeeIt=true

				if not myRS.Fields("OBS1REPT") is nothing then
					if not myRS.Fields("OBS1REPT").IsNull and not myRS.Fields("OBS1REPT")=-1  then
						ThisForm.Pages("PAGE1").Controls("txtObsRept").Value=CStr(myRS.Fields("OBS1REPT"))
					else
						ThisForm.Pages("PAGE1").Controls("txtObsRept").Value=""
					end if
				end if

			end if
		end if

		SetControlNotSelected "frmMarkAnimal","PAGE1","btnPilotSawIt"
		SetControlNotSelected "frmMarkAnimal","PAGE1","btnObsSawIt"
		SetControlNotSelected "frmMarkAnimal","PAGE1","btnBothSawIt"

		if DidPilotSeeIt=true and DidObsSeeIt=true then
			SetControlSelected "frmMarkAnimal","PAGE1","btnBothSawIt",""
		elseif DidPilotSeeIt = true then
			SetControlSelected "frmMarkAnimal","PAGE1","btnPilotSawIt",""
		elseif DidObsSeeIt = true then
			SetControlSelected "frmMarkAnimal","PAGE1","btnObsSawIt",""
		end if


		if not myRS.Fields("SPECIES") is nothing then
			if not myRS.Fields("SPECIES").IsNull then
				SetListIndexByItemText myRS.Fields("SPECIES").Value,ThisForm.Pages("PAGE1").Controls("lstAnimalList")
			end if
		end if
		if not myRS.Fields("GROUPSIZE") is nothing then
			if not myRS.Fields("GROUPSIZE").IsNull and not myRS.Fields("GROUPSIZE")=-1 then
				ThisForm.Pages("PAGE1").Controls("txtGroupSize").Value=CStr(myRS.Fields("GROUPSIZE").Value)
			end if
		end if
		if not myRS.Fields("GROUPTYPE") is nothing then
			if not myRS.Fields("GROUPTYPE").IsNull then
				SetListIndexByItemText myRS.Fields("GROUPTYPE").Value,ThisForm.Pages("PAGE1").Controls("lstGroupType")
			end if
		end if
		if not myRS.Fields("ACTIVITY") is nothing then
			if not myRS.Fields("ACTIVITY").IsNull then
				SetListIndexByItemText myRS.Fields("ACTIVITY").Value,ThisForm.Pages("PAGE1").Controls("lstActivity")
			end if
		end if
		if not myRS.Fields("PCTCOVER") is nothing then
			if not myRS.Fields("PCTCOVER").IsNull and not myRS.Fields("PCTCOVER")=-1 then
				ThisForm.Pages("PAGE1").Controls("txtPerCover").Value=myRS.Fields("PCTCOVER").Value
			end if
		end if
		if not myRS.Fields("PCTSNOW") is nothing then
			if not myRS.Fields("PCTSNOW").IsNull and not myRS.Fields("PCTSNOW")=-1 then
				ThisForm.Pages("PAGE1").Controls("txtSnow").Value=myRS.Fields("PCTSNOW").Value
			end if
		end if
		if not myRS.Fields("AnimalID") is nothing then
			if not myRS.Fields("AnimalID").IsNull then
				ThisForm.Pages("PAGE1").Controls("txtAnimalID").Value=myRS.Fields("AnimalID").Value
			end if
		end if
		if not myRS.Fields("TransectID") is nothing then
			if not myRS.Fields("TransectID").IsNull then
				ThisForm.Pages("PAGE1").Controls("txtTransectID").Value=myRS.Fields("TransectID").Value
			end if
		end if


		set myRS=nothing
		set ThisForm=nothing
		set lyrAnimal=nothing

end sub

'============================================================================================
'============================================================================================
sub UpdateMarkAnimal(ThisBookmark)

	dim lyrAnimal,lyrTrnOrig
	dim ErrorMessage,Bookmark
	dim myRS,TransectID,SegmentID,SurveyID
	dim Species,GroupSize,GroupType,Activity,Cover,Snow
	dim ThisForm,Latitude,Longitude
	DIM X,Y,Z,Altitude,ThisDate,ViewSide
	dim ThisPoint,AnimalID
	Dim DidPilotSeeIt,DidObsSeeIt,RepValue


	'make sure the layer is in the map
	set lyrAnimal = Application.Map.Layers("animals")
	if lyrAnimal is nothing then
		msgbox "Could not find the Animal layer"
		exit sub
	end if

	'make the layer editable to do select
	if lyrAnimal.CanEdit=true then 
		lyrAnimal.Editable=true
	else	
		msgbox "not editable"
		exit sub
	end if


	set myRS=lyrAnimal.Records
	Bookmark=CLng(ThisBookmark)
	myRS.Bookmark=Bookmark


	'get all the selected values for the animal sitting
	set ThisForm=Applet.Forms("frmMarkAnimal")
	Species=ThisForm.Pages("PAGE1").Controls("lstAnimalList").Text
	GroupSize=ThisForm.Pages("PAGE1").Controls("txtGroupSize").Text

	GroupType=ThisForm.Pages("PAGE1").Controls("lstGroupType").Text


	Activity=ThisForm.Pages("PAGE1").Controls("lstActivity").Text
	Cover=ThisForm.Pages("PAGE1").Controls("txtPerCover").Text
	Snow=ThisForm.Pages("PAGE1").Controls("txtSnow").Text
	

	DidPilotSeeIt=false
	DidObsSeeIt=false

	'if the pilot saw the animal, collect pilot information
	if  GetControlSelectedState("frmMarkAnimal","PAGE1","btnPilotSawIt")=true then
		DidPilotSeeIt=true 
	end if

	'if the observer saw the animal collect observer's information
	if  GetControlSelectedState("frmMarkAnimal","PAGE1","btnObsSawIt")=true then
		DidObsSeeIt=true
	end if


	'if both saw the animal collect all information
	if  GetControlSelectedState("frmMarkAnimal","PAGE1","btnBothSawIt")=true then
		DidPilotSeeIt=true 
		DidObsSeeIt=true
	end if 


	if DidPilotSeeIt=true then

		RepValue=ThisForm.Pages("PAGE1").Controls("txtPilotRept").Text

		if not myRS.Fields("PILOTLNAM") is nothing then
			myRS.Fields("PILOTLNAM").Value=Application.UserProperties("Pilot")
		end if
		if not myRS.Fields("PILOTDIR") is nothing then		
			myRS.Fields("PILOTDIR").Value=Application.UserProperties("PilotDir")
		end if
		if not myRS.Fields("PILOTREPT") is nothing then
			if IsNumeric(RepValue) then 
				myRS.Fields("PILOTREPT").Value=CInt(RepValue)
			end if
		end if

	else
		if not myRS.Fields("PILOTLNAM") is nothing then
			myRS.Fields("PILOTLNAM").Value=""
		end if
		if not myRS.Fields("PILOTDIR") is nothing then
			myRS.Fields("PILOTDIR").Value=""
		end if
		if not myRS.Fields("PILOTREPT") is nothing then
				myRS.Fields("PILOTREPT").Value=-1
		end if

	end if

	if DidObsSeeIt=true then

		RepValue=ThisForm.Pages("PAGE1").Controls("txtObsRept").Text

		if not myRS.Fields("OBS1LNAM") is nothing then
			myRS.Fields("OBS1LNAM").Value=Application.UserProperties("Observer1")
		end if
		if not myRS.Fields("OBS1DIR") is nothing then
			myRS.Fields("OBS1DIR").Value=Application.UserProperties("ObsDir1")
		end if
		if not myRS.Fields("OBS1REPT") is nothing then
			if IsNumeric(RepValue) then 
				myRS.Fields("OBS1REPT").Value=CInt(RepValue)
			end if
		end if

		if not Application.UserProperties("Observer2")="" then

			if not myRS.Fields("OBS2LNAM") is nothing then
				myRS.Fields("OBS2LNAM").Value=Application.UserProperties("Observer2")
			end if
			if not myRS.Fields("OBS2DIR") is nothing then
				myRS.Fields("OBS2DIR").Value=Application.UserProperties("ObsDir2")
			end if
			if not myRS.Fields("OBS2REPT") is nothing then
				if IsNumeric(RepValue) then 
					myRS.Fields("OBS2REPT").Value=CInt(RepValue)
				end if
			end if

		end if


	else

		if not myRS.Fields("OBS1LNAM") is nothing then
			myRS.Fields("OBS1LNAM").Value=""
		end if
		if not myRS.Fields("OBS1DIR") is nothing then
			myRS.Fields("OBS1DIR").Value=""
		end if
		if not myRS.Fields("OBS1REPT") is nothing then
			myRS.Fields("OBS1REPT").Value=-1
		end if

		if not Application.UserProperties("Observer2")="" then

			if not myRS.Fields("OBS2LNAM") is nothing then
				myRS.Fields("OBS2LNAM").Value=""
			end if
			if not myRS.Fields("OBS2DIR") is nothing then
				myRS.Fields("OBS2DIR").Value=""
			end if
			if not myRS.Fields("OBS2REPT") is nothing then
				myRS.Fields("OBS2REPT").Value=-1
			end if

		end if

	end if


	'about the animal
	if not myRS.Fields("SPECIES") is nothing then
		myRS.Fields("SPECIES").Value=Species
	end if
	if not myRS.Fields("ANIMALTYPE") is nothing then
		myRS.Fields("ANIMALTYPE").Value=Species
	end if
	if not myRS.Fields("GROUPSIZE") is nothing then
		if IsNumeric(GroupSize) then
			myRS.Fields("GROUPSIZE").Value=CInt(GroupSize)
		else	
			myRS.Fields("GROUPSIZE").Value=-1
		end if
	end if
	if not myRS.Fields("GROUPTYPE") is nothing then
		if not GroupType="" then
			myRS.Fields("GROUPTYPE").Value=GroupType
		else
			myRS.Fields("GROUPTYPE").Value=""
		end if
	end if
	if not myRS.Fields("ACTIVITY") is nothing then
		if not Activity="" then
			myRS.Fields("ACTIVITY").Value=Activity
		else
			myRS.Fields("ACTIVITY").Value=""
		end if
	end if
	if not myRS.Fields("PCTCOVER") is nothing then
		if IsNumeric(Cover) then
			myRS.Fields("PCTCOVER").Value=CInt(Cover)
		else
			myRS.Fields("PCTCOVER").Value=-1
		end if
	end if
	if not myRS.Fields("PCTSNOW") is nothing then
		if IsNumeric(Snow) then
			myRS.Fields("PCTSNOW").Value=CInt(Snow)
		else
			myRS.Fields("PCTSNOW").Value=-1
		end if
	end if


	myRS.Update


	lyrAnimal.Editable=false

	set ThisForm=nothing
	set myRS=nothing
	set lyrAnimal=nothing


end sub

'============================================================================================
'============================================================================================
sub btnUpdatePosition_Clicked

	dim lyrAnimal,ThisAnimalBookmark,myRS
	dim ThisPoint,Altitude,Latitude,Longitude

	if Application.UserProperties("AnimalBookmark")="" then
		msgbox "You must click 'Mark Animal' to create an animal record before you can update it's position."
		exit sub
	end if

	ThisAnimalBookmark=Application.UserProperties("AnimalBookmark")
	'make sure the layer is in the map
	set lyrAnimal = Application.Map.Layers("animals")
	if lyrAnimal is nothing then
		msgbox "Could not find the Animal layer"
		exit sub
	end if


	lyrAnimal.Editable=true

	set ThisPoint=Application.CreateAppObject ("Point")
	ThisPoint.X=GPS.X
	ThisPoint.Y=GPS.Y
	ThisPoint.Z=GPS.Z
	Altitude=GPS.Altitude
	Latitude=GPS.Latitude
	Longitude=GPS.Longitude


	set myRS=lyrAnimal.Records
	myRS.Bookmark=CLng(ThisAnimalBookmark)
	

	set myRS.Fields.Shape=ThisPoint


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
	if not myRS.Fields("DIST2TRANS") is nothing then
		myRS.Fields("DIST2TRANS").Value=DistanceFromLastSegment(ThisPoint)
	end if


	myRS.Update	
	
	lyrAnimal.Editable=false
	Map.Redraw true

	set myRS=nothing
	set lyrAnimal=nothing
	set ThisPoint=nothing

end sub

'============================================================================================
'============================================================================================
sub btnMarkHorizon_Click

	Dim Bookmark
	dim lyrAnimal
	dim myRS,HrznRS
	dim ErrorMessage
	dim Lat,Lon,ThisPoint
	Dim lyrHorizon,AnimalID
	Dim SurveyID,SegmentID,TransectID,HorizonID
	Dim TotalRecordsWithSameHorizon

	'make sure our gps is active
	ErrorMessage=CheckGPS()
	if not ErrorMessage="" then
		msgbox ErrorMessage
		exit sub
	end if

	
	'make sure the animal layer is in the map
	set lyrAnimal = Application.Map.Layers("animals")
	if lyrAnimal is nothing then
		msgbox "Could not find the Animal layer"
		exit sub
	end if

	'make sure the horizon layer is in the map
	set lyrHorizon = Application.Map.Layers("Horizon")
	if lyrHorizon is nothing then
		msgbox "Could not find the Horizon layer"
		exit sub
	end if

	'make sure we have a current animal record
	if Application.UserProperties("AnimalBookmark")="" then
		msgbox "You will need to mark an animal before marking a horizon."
		exit sub
	end if

	'get the current globals used to identify the current session
	SegmentID=CLng(Application.UserProperties("SegmentID"))
	TransectID=Application.UserProperties("TransectID")
	SurveyID=Application.UserProperties("SurveyID")
	HorizonID=Application.UserProperties("HorizonID")

	'create a point representing the current gps position
	set ThisPoint=Application.CreateAppObject ("Point")
	ThisPoint.X=GPS.X
	ThisPoint.Y=GPS.Y
	ThisPoint.Z=GPS.Z
	Lat=GPS.Latitude
	Lon=GPS.Longitude

	'make animal layer editable so we can edit animal data
	call RemoveAllLayersFromEditMode
	lyrAnimal.Editable=true
	
	'move to current animal sighting record
	Bookmark=CLng(Application.UserProperties("AnimalBookmark"))
	set myRS=lyrAnimal.Records
	myRS.Bookmark=Bookmark


	'if we have a horizon field...
	if not myRS.Fields("HorizonID") is nothing then
		'if the horizon field is not null...
		if not myRS.Fields("HorizonID").IsNull then
			TotalRecordsWithSameHorizon=GetHorizonReferenceCount(CStr(myRS.Fields("HorizonID").Value),TransectID,SurveyID)
			myRS.Bookmark=Bookmark
			if TotalRecordsWithSameHorizon<2 then
				msgbox "There already exists a Horizon for the currently selected animal sighting." & _
				vbCrLf & " Click Update Horizon if you wish to change the Horizon position."
				set myRS=nothing
				lyrAnimal.Editable=false
				exit sub

			else
				
				myRS.Fields("HorizonID").Value=CLng(HorizonID)
			end if
		else
			myRS.Fields("HorizonID").Value=CLng(HorizonID)
		end if
	end if

	
	myRS.Update


	call RemoveAllLayersFromEditMode
	lyrHorizon.Editable=true

	set HrznRS=lyrHorizon.Records

	HrznRS.AddNew ThisPoint

	Application.UserProperties("HorizonBookmark")=CStr(HrznRS.Bookmark)

	if not HrznRS.Fields("HorizonID") is nothing then
		HrznRS.Fields("HorizonID").Value=CLng(HorizonID)
	end if

	if not HrznRS.Fields("SegmentID") is nothing then
		HrznRS.Fields("SegmentID").Value=CLng(SegmentID)
	end if


	if not HrznRS.Fields("SurveyID") is nothing then
		HrznRS.Fields("SurveyID").Value=CLng(SurveyID)
	end if

	if not HrznRS.Fields("TransectID") is nothing then
		HrznRS.Fields("TransectID").Value=CLng(TransectID)
	end if

	if not HrznRS.Fields("DistToSeg") is nothing then
		HrznRS.Fields("DistToSeg").Value=DistanceFromLastSegment(ThisPoint)
	end if

	if not HrznRS.Fields("HorizonX") is nothing then
		HrznRS.Fields("HorizonX").Value=ThisPoint.X
	end if

	if not HrznRS.Fields("HorizonY") is nothing then
		HrznRS.Fields("HorizonY").Value=ThisPoint.Y
	end if

	if not HrznRS.Fields("HorizonLat") is nothing then
		HrznRS.Fields("HorizonLat").Value=Lat
	end if

	if not HrznRS.Fields("HorizonLng") is nothing then
		HrznRS.Fields("HorizonLng").Value=Lon
	end if

	HrznRS.Update
	lyrHorizon.Editable=false



	HorizonID=HorizonID+1
	Application.UserProperties("HorizonID")=CLng(HorizonID)

	set ThisPoint=nothing
	set HrznRS=nothing
	set lyrAnimal=nothing
	set lyrHorizon=nothing
	set myRS=nothing

end sub

'============================================================================================
'============================================================================================
sub btnUpdateHorizon_Clicked

	Dim Bookmark
	dim lyrAnimal
	dim myRS,HrznRS
	dim ErrorMessage
	dim Lat,Lon,ThisPoint
	Dim lyrHorizon,AnimalID
	Dim SurveyID,SegmentID,TransectID,HorizonID
	Dim HorizonBookmark

	ErrorMessage=CheckGPS()
	if not ErrorMessage="" then
		msgbox ErrorMessage
		exit sub
	end if

	

	'make sure the animal layer is in the map
	set lyrAnimal = Application.Map.Layers("animals")
	if lyrAnimal is nothing then
		msgbox "Could not find the Animal layer"
		exit sub
	end if

	'make sure the horizon layer is in the map
	set lyrHorizon = Application.Map.Layers("Horizon")
	if lyrHorizon is nothing then
		msgbox "Could not find the Horizon layer"
		exit sub
	end if

	if Application.UserProperties("AnimalBookmark")="" then
		msgbox "You will need to mark an animal before marking a horizon."
		exit sub
	end if

	SegmentID=CLng(Application.UserProperties("SegmentID"))
	TransectID=Application.UserProperties("TransectID")
	SurveyID=Application.UserProperties("SurveyID")


	set ThisPoint=Application.CreateAppObject ("Point")
	ThisPoint.X=GPS.X
	ThisPoint.Y=GPS.Y
	ThisPoint.Z=GPS.Z
	Lat=GPS.Latitude
	Lon=GPS.Longitude

	call RemoveAllLayersFromEditMode
	lyrAnimal.Editable=true
	
	Bookmark=CLng(Application.UserProperties("AnimalBookmark"))

	set myRS=lyrAnimal.Records
	myRS.Bookmark=Bookmark



	if not myRS.Fields("HorizonID") is nothing then
		if myRS.Fields("HorizonID").IsNull then

			msgbox "There is no Horizon for this animal sighting to perform an update on." & _
			vbCrLf & " Click Mark Horizon if you wish to create a Horizon."
			set myRS=nothing
			lyrAnimal.Editable=false
			exit sub
		else
			HorizonID=myRS.Fields("HorizonID").Value
		end if
	end if

	set myRS=nothing

	call RemoveAllLayersFromEditMode
	lyrHorizon.Editable=true

	set HrznRS=lyrHorizon.Records

	HorizonBookmark=GetHorizonBookmark(CStr(HorizonID),TransectID,SurveyID)
	HrznRS.Bookmark=HorizonBookmark

	set HrznRS.Fields.Shape=ThisPoint

	if not HrznRS.Fields("DistToSeg") is nothing then
		HrznRS.Fields("DistToSeg").Value=DistanceFromLastSegment(ThisPoint)
	end if

	if not HrznRS.Fields("HorizonX") is nothing then
		HrznRS.Fields("HorizonX").Value=ThisPoint.X
	end if

	if not HrznRS.Fields("HorizonY") is nothing then
		HrznRS.Fields("HorizonY").Value=ThisPoint.Y
	end if

	if not HrznRS.Fields("HorizonLat") is nothing then
		HrznRS.Fields("HorizonLat").Value=Lat
	end if

	if not HrznRS.Fields("HorizonLng") is nothing then
		HrznRS.Fields("HorizonLng").Value=Lon
	end if

	HrznRS.Update

	Map.Refresh true
	lyrHorizon.Editable=false
	

	set HrznRS=nothing
	set lyrHorizon=nothing
	set myRS=nothing
	set lyrAnimal=nothing
	set ThisPoint=nothing

end sub

'============================================================================================
'============================================================================================
sub btnUseLastHorizon_Clicked

	Dim ErrorMessage,HorizonID,lyrAnimal,Bookmark,myRS,AnimalBookmark,lyrHorizon
	Dim HorizonBK,TotalRecordsWithSameHorizon,DeleteHorizon,HorizonBookmark
	dim HrznRS,OldHorizonID,SegmentID,TransectID,SurveyID


	ErrorMessage=CheckGPS()
	if not ErrorMessage="" then
		msgbox ErrorMessage
		exit sub
	end if

	HorizonID=-1
	DeleteHorizon=false

	HorizonBK=Application.UserProperties("HorizonBookmark")
	if HorizonBK="" then
		msgbox "No previous Horizons for this off-transect segment exists for resuse."
		exit sub
	end if

	AnimalBookmark=Application.UserProperties("AnimalBookmark")
	if AnimalBookmark="" then
		msgbox "You will need to mark an animal before setting it's Horizon."
		exit sub
	end if

	'make sure the animal layer is in the map
	set lyrAnimal = Application.Map.Layers("animals")
	if lyrAnimal is nothing then
		msgbox "Could not find the Animal layer"
		exit sub
	end if



	'make sure the horizon layer is in the map
	set lyrHorizon = Application.Map.Layers("Horizon")
	if lyrHorizon is nothing then
		msgbox "Could not find the Horizon layer"
		exit sub
	end if

	set myRS=lyrHorizon.Records
	myRS.Bookmark=HorizonBK
	
	if not myRS.Fields("HorizonID") is nothing then
		HorizonID= myRS.Fields("HorizonID").Value
	end if

	set myRS=nothing

	'make the layer editable to do select
	if lyrAnimal.CanEdit=true then 
		lyrAnimal.Editable=true
	else	
		msgbox "not editable"
		exit sub
	end if

	SegmentID=CLng(Application.UserProperties("SegmentID"))
	TransectID=Application.UserProperties("TransectID")
	SurveyID=Application.UserProperties("SurveyID")

	Bookmark=CLng(AnimalBookmark)
	
	set myRS=lyrAnimal.Records
	myRS.Bookmark=Bookmark

	if not myRS.Fields("HorizonID") is nothing then

		if not myRS.Fields("HorizonID").IsNull then

			
			if CStr(myRS.Fields("HorizonID").Value)=CStr(HorizonID) then
				msgbox "This animal sighting already has the last Horizon set as it's Horizon."
				set myRS=nothing
				lyrAnimal.Editable=false
				exit sub
			end if

			TotalRecordsWithSameHorizon=GetHorizonReferenceCount(CStr(myRS.Fields("HorizonID").Value),TransectID,SurveyID)
			'myRS.Bookmark=Bookmark

			OldHorizonID=CStr(myRS.Fields("HorizonID").Value)
			if TotalRecordsWithSameHorizon=1 then
				DeleteHorizon=true
			else
				DeleteHorizon=false
			end if

			myRS.Fields("HorizonID").Value=CLng(HorizonID)
		else
			myRS.Fields("HorizonID").Value=CLng(HorizonID)
		end if
	end if

	myRS.Update
	set myRS=nothing
	
	lyrAnimal.Editable=false

	if DeleteHorizon=true then
	
		lyrHorizon.Editable=true

		set HrznRS=lyrHorizon.Records
		HorizonBookmark=GetHorizonBookmark(OldHorizonID,TransectID,SurveyID)
		HrznRS.Bookmark=HorizonBookmark
		HrznRS.Delete
		HrznRS.Update
		HrznRS.Pack
		
		set HrznRS=nothing
		lyrHorizon.Editable=false

	end if


	set lyrHorizon=nothing
	set myRS=nothing
	set lyrAnimal=nothing
	set HrznRS=nothing


	Map.Refresh true

end sub


'============================================================================================
'============================================================================================
sub frmMarkAnimal_OkClick

	Dim ErrorMessage

	'save the current animal record form data if we have one
	if not Application.UserProperties("AnimalBookmark")="" then
		UpdateMarkAnimal Application.UserProperties("AnimalBookmark")
	end if

	ErrorMessage=ValidateBearFields(AnimalBKPerOffSegment)
	if not ErrorMessage="" then
		msgbox ErrorMessage
		exit sub
	end if

	'end this off-transect session and clean up
	call ResumeTransect

	'close the animal form
	Applet.Forms("frmMarkAnimal").Close

end sub


'============================================================================================
'============================================================================================
sub ResumeTransect

	dim NewRecordBK
	dim AnimalForm
	dim lyrAnimal,X,Y,myRS,Current_Value
	dim ThisForm
	dim ThisPage
	Dim ErrorMessage,SegmentID
	dim SurveyID,TransectID
	Dim GPSPoint

	'clear the current sighting and horizon since these don't carry over to the next segment
	Application.UserProperties("AnimalBookmark")=""
	Application.UserProperties("HorizonBookmark")=""


	'reset animal list array, also doesn't continue to the next off-transect
	redim AnimalBKPerOffSegment(0)
	AnimalBKPerOffSegment(0)=""

	
	'we pause the transect mode so that no new points will be added to the segment vertex collection
	'since we have ended the segment
	Application.UserProperties("Transect_Mode")="Pause"

	'get the current gps point
	set GPSPoint=Application.CreateAppObject ("Point")
	GPSPoint.X=GPS.X
	GPSPoint.Y=GPS.Y
	GPSPoint.Z=GPS.Z

	'get current transect properties
	TransectID=Application.UserProperties("TransectID")
	SurveyID=Application.UserProperties("SurveyID")
	SegmentID=CLng(Application.UserProperties("SegmentID"))

	'create the segment using the current gps point as it's end point
	ErrorMessage=MakeLineFromGPSPointsAndAddToLayer(GPSPoint,"TrackLog",SurveyID,TransectID,CStr(SegmentID),"OffTransect")
	
	'since we are resuming an off-transect segment, this new on-transect segment will start a new off/on transect
	'segment pair.off transect segments always share the same segment id with the previous on-transect segment
	SegmentID=SegmentID + 1
	Application.UserProperties("SegmentID")=SegmentID

	'change the segtype and display segtype to on-trasect since we are back on the transect
	Application.UserProperties("SegType")="OnTransect"
	SetDisplayValue "Transect Status","On"

	'delete all the backup points and use the current gps point as the first of the new backup points
	DeleteAllBackupLogFilePoints Application.Path & "\Applets\PointLog.dbf"
	AddPointToBackupLog Application.Path & "\Applets\PointLog.dbf",GPSPoint.X, _ 
		GPSPoint.Y,GPSPoint.Z,SurveyID,TransectID,SegmentID,"OnTransect"

	'use the current gps point as the first of the new collection points for the next segment
	RestartGlobalCollectionWithCurrentGPSPoint GPSPoint.X, _ 
		GPSPoint.Y,GPSPoint.Z

	'resume transect mode so that gps points will be added to our segment vertex collection again
	Application.UserProperties("Transect_Mode")="Start"


	'make the desired toolbottons visible
	Call DisableTransectButtons
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnMarkAnimal").Enabled=true
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnEndTransect").Enabled=true
	set GPSPoint=nothing


end sub


'============================================================================================
'============================================================================================
sub tlbtnEndTransect_Click
	
	Dim ErrorMessage,SegmentID,TransectID,SurveyID,myRS
	Dim GPSPoint

	

	ErrorMessage=CheckGPS()
	if not ErrorMessage="" then
		msgbox ErrorMessage
		exit sub
	end if
	
	set GPSPoint=Application.CreateAppObject ("Point")

	'get the current segment's id (this is always one more than the global seg id)
	SegmentID=CLng(Application.UserProperties("SegmentID"))


	'get current transect properties
	TransectID=Application.UserProperties("TransectID")
	SurveyID=Application.UserProperties("SurveyID")
	Application.UserProperties("Transect_Mode")="Stop"

	'get the gps point at the time when end transect was clicked
	GPSPoint.X=GPS.X
	GPSPoint.Y=GPS.Y
	GPSPoint.Z=GPS.Z

	'create the segment
	ErrorMessage=MakeLineFromGPSPointsAndAddToLayer(GPSPoint,"TrackLog",SurveyID,TransectID,CStr(SegmentID),"OnTransect")

	'delete the backup points for the segment
	DeleteAllBackupLogFilePoints Application.Path & "\Applets\PointLog.dbf"

	'update the current transect flown status to 'Y' , flown
	if not Map.Layers("TrnOrig.shp") is nothing then
		Map.Layers("TrnOrig.shp").Editable=true
		set myRS=Map.Layers("TrnOrig.shp").Records  
		myRS.Bookmark=myRS.Find("[TransectID]=" & TransectID) 
		myRS.Fields("Flown").Value="Y"
		myRS.Update
		set myRS=nothing
		Map.Layers("TrnOrig.shp").Editable=false	
	end if

	'reset segment id
	Application.UserProperties("SegmentID")=1
	Application.UserProperties("AnimalID")=1
	Application.UserProperties("HorizonID")=1
	Application.UserProperties("SegType")=""
	

	'reset display
	SetDistanceFromLog Application.Path & _ 
			"\Applets\Survey\defaultvalues.dbf",0
	Application.UserProperties("OnTransLength")=0
	SetDisplayValue "Transect Status","---"


	Application.UserProperties("ExtendedTransect")=""

	
	Application.Toolbars("tlbNPSDisplay").Caption="Distance Traveled: 0.000 km   |   " _
		& "Transect Status: ---   |   Transect ID: ---   |"

	call DisableTransectButtons
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransect").Enabled=true
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnSelectTransectDown").Enabled=true
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnLand").Enabled=true


	'open end transect form to set weather,etc.
	'Applet.Forms("frmEndTransect").Show
	Applet.Forms("frmWeather").Show


	set GPSPoint=nothing
	set myRS=nothing

end sub

'============================================================================================
'============================================================================================
sub GPS_GetPosition

	Dim TransectMode
	Dim GPSPoint
	Dim SurveyID,TransectID,SegmentID,SegType
	Dim DistanceFromLastPoint
	Dim OnTransLength,TransLength

	if GPS.IsOpen=false then
		exit sub
	end if

	set GPSPoint=Application.CreateAppObject ("Point")
	GPSPoint.X = GPS.X
	GPSPoint.Y = GPS.Y
	GPSPoint.Z = GPS.Z


	AddGPSPointToGPSLog Application.Path & "\Applets\Survey\GPSPointsLog.shp",GPSPoint

	'this recenters the map on the gps point
	'Map.CenterAtXY GPS.X,GPS.Y


	'if we have a current animal bookmark, draw a blue rectangle around it
	if not Application.UserProperties("AnimalBookmark")="" then
		DrawRectOnSelAnimal Application.UserProperties("AnimalBookmark")
	end if
	

	'only collect tracklog points if a transect is being flown
	TransectMode = Application.UserProperties("Transect_Mode")
	if not TransectMode="Start" then
		set GPSPoint=nothing
		exit sub
	end if



	'get the current transect's properties
	TransectID=Application.UserProperties("TransectID")
	SurveyID=Application.UserProperties("SurveyID")


	'get the current segment's properties
	SegType=Application.UserProperties("SegType")
	SegmentID=CLng(Application.UserProperties("SegmentID"))
 


	if GPSPoints_Collection.Count>0 then

		'get the distance from the last ontransect gps point to the current ontransect gps point
		DistanceFromLastPoint=DistanceFromLastPointInCollection(GPSPoint)
			
		'if spike occurs, don't add point to collection or log
		if DistanceFromLastPoint>1000 then
			set GPSPoint=nothing
			exit sub
		end if

	end if

	'save the current point and all its properties to the backup point log
	AddPointToBackupLog Application.Path & "\Applets\PointLog.dbf",GPSPoint.X, _ 
	GPSPoint.Y,GPSPoint.Z,SurveyID,TransectID,SegmentID,SegType

	'add the current point to the current segment's point collection
	GPSPoints_Collection.Add GPSPoint

	'do distance calculation for on transect segments
	if SegType="OnTransect" and GPSPoints_Collection.Count>0 then

		
		'check if distance traveled is greater than the length of the transect
		OnTransLength=CDbl(Application.UserProperties("OnTransLength"))
		TransLength=Application.UserProperties("TransLength")
		if OnTransLength=>TransLength  and Application.UserProperties("ExtendedTransect")="" then

			Application.UserProperties("ExtendedTransect")="Extend"

			if Applet.Forms("frmAlertEndTransect").Mode = -1 then
				Applet.Forms("frmAlertEndTransect").Show
			end if
		end if

		'update the ontransect length global variable
		Application.UserProperties("OnTransLength")=OnTransLength+(DistanceFromLastPoint)
			
		'the current on transect distance is stored in the defaultvalues dbf each time it's
		'calculated. If an error occurs and arcpad is restarted, the distance will continue
		'from the last value in the dbf
		SetDistanceFromLog Application.Path & _ 
			"\Applets\Survey\defaultvalues.dbf",OnTransLength+(DistanceFromLastPoint)
 
		'update the display with the current distance
		'Application.Toolbars("tlbNPSDisplay").Caption="Distance Traveled:" _ 
		'& Round((CDbl(Application.UserProperties("OnTransLength"))/1000),3) & " km"
			
	end if

	SetDisplayValue "Distance Traveled", _
		Round((CDbl(Application.UserProperties("OnTransLength"))/1000),3) & " km"
	

	set GPSPoint=nothing


end sub

'============================================================================================
'============================================================================================
sub tlbtnEndLand_Click

	Application.UserProperties("strMode")=""
	call DisableTransectButtons
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnLoadMap").Enabled=true
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnLand").Enabled=true

	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotRight").Visible=false
	Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotLeft").Visible=false


end sub

'============================================================================================
'function that runs when the user clicks the find transect button
'============================================================================================
sub tlbtnFindTransect_Click
	Applet.Forms("frmFindTransect").Show
end sub

'============================================================================================
'handles all Find Transect form events
'============================================================================================
sub FindTransect_Event(sender)

		Dim ErrorMessage
		dim ThisForm
		dim CurrentValue


		

		set ThisForm=Applet.Forms("frmFindTransect")

		'=======================================================
		'=======================================================
		if sender="load" then
				ThisForm.Pages("PAGE1").Controls("btnFindTransect").SetFocus
		end if

		'=======================================================
		'=======================================================
		if sender="close" then
			ThisForm.Close
		end if

		'=======================================================
		'=======================================================
		if sender="easynumber" then
			CurrentValue=ThisForm.Pages("PAGE1").Controls("txtTransectID").Value
			CurrentValue=GetEasyNumber(CurrentValue)
			if not CurrentValue="cancel" then
				ThisForm.Pages("PAGE1").Controls("txtTransectID").Value=CurrentValue
			end if
		end if

		'=======================================================
		'=======================================================
		if sender ="find" then

			ErrorMessage=""

			CurrentValue=ThisForm.Pages("PAGE1").Controls("txtTransectID").Value

			if not IsNumeric(CurrentValue) then
				ErrorMessage="Please enter a valid number"
			end if
			
			if ErrorMessage="" then
				ErrorMessage=HighlightTransect(CurrentValue,false)
			end if


			if not ErrorMessage="" then
				msgbox ErrorMessage
			else
				ThisForm.Close
			end if
		end if



		set ThisForm=nothing

end sub

'============================================================================================
'opens the easynumbers form, modally and returns the value the user entered
'============================================================================================
function GetEasyNumber(startValue)

	Application.UserProperties("EasyNumber")=startValue

	Applet.Forms("frmEasyNumbers").Show

	GetEasyNumber=Application.UserProperties("EasyNumber")

end function

'============================================================================================
'handles all events for the easy numbers form
'============================================================================================
sub EasyNumbers_Click(sender)

	dim ThisForm,CurrentValue


	set ThisForm=Applet.Forms("frmEasyNumbers")

	if sender="load" then		
		ThisForm.Pages("PAGE1").Controls("txtValue").Value=Application.UserProperties("EasyNumber")
	end if
	
	if IsNumeric(sender)=true then
		ThisForm.Pages("PAGE1").Controls("txtValue").Value= ThisForm.Pages("PAGE1").Controls("txtValue").Value & sender	
	end if

	if sender="clr" then
		ThisForm.Pages("PAGE1").Controls("txtValue").Value=""
	end if

	if sender="bck" then
			CurrentValue=ThisForm.Pages("PAGE1").Controls("txtValue").Value
			if len(CurrentValue)<2 then 
				ThisForm.Pages("PAGE1").Controls("txtValue").Value=""
			else
				ThisForm.Pages("PAGE1").Controls("txtValue").Value=Left(CurrentValue,len(CurrentValue)-1)
			end if		
	end if
	

	if sender="ok" then
		Application.UserProperties("EasyNumber")=ThisForm.Pages("PAGE1").Controls("txtValue").Value
		Applet.Forms("frmEasyNumbers").Close
	end if

	if sender="cancel" then
		Application.UserProperties("EasyNumber")="cancel"
		Applet.Forms("frmEasyNumbers").Close
	end if

	set ThisForm=nothing

end sub

'============================================================================================
'handles all events for the sheep form
'============================================================================================
sub frmSheepForm_Event(SenderName,EventName)

	dim ThisForm,ThisFormName
	dim ControlNameArray(12)
	dim FocusIndex
	dim FocusField
	dim SFManager
	dim NewBookmark
	dim ErrorMessage
	dim TransectID,SegmentID,SurveyID,AnimalID
	dim CurrentAnimalBK,GoToAnimalBK,CanNav
	dim ChangeFieldValue,Total,Index,AnimalSum
	dim AllOK,PrevAnimalBK,CurAnimalID
	Dim CalValue
	
	'get form name - may be long or short form
	ThisFormName=ThisEvent.Object.Name
	if ThisFormName <> "frmSheepForm" and ThisFormName <> "frmSheepFormLong" then
		'get the control's parent which is the page then get the pages parent which
		'is the form. then get the form's name
		ThisFormName=ThisEvent.Object.Parent.Parent.Name
	end if


	'an array of all the editable fields
	if ThisFormName <> "frmSheepFormLong" then 
		ControlNameArray(0)="txtEweLike"
		ControlNameArray(1)="txtLambs"
		ControlNameArray(2)="txtLT_FullCurlRam"
		ControlNameArray(3)="txtGT_FullCurlRam"
		ControlNameArray(4)="txtUnclassifiedRams"
		ControlNameArray(5)="txtUnclassifiedSheep"
	else
		ControlNameArray(0)="txtEwes"
		ControlNameArray(1)="txtLambs"
		ControlNameArray(2)="txtLT_FullCurlRam"
		ControlNameArray(3)="txtGT_FullCurlRam"
		ControlNameArray(4)="txtUnclassifiedRams"
		ControlNameArray(5)="txtUnclassifiedSheep"
		ControlNameArray(6)="txtEweLike"
		ControlNameArray(7)="txtYrlgs"
		ControlNameArray(8)="txtRams_LTHalf"
		ControlNameArray(9)="txtRams_Half"
		ControlNameArray(10)="txtRams_3_qtrs"
		ControlNameArray(11)="txt_7_Eighths"
	end if

	set SFManager=new SheepFormManager
	SFManager.Init ThisFormName

	'name of value without 3 letter prefix -used to get number of the calculator button clicked	
	CalValue=""
	if Len(SenderName)>3 then
		CalValue=Mid(SenderName,4)
	end if

	

	set ThisForm=Applet.Forms(ThisFormName)


	
	'===========================================================
	'called when the the form is shown for the first time, clears
	'all the values in the data fields
	'===========================================================
	if EventName="onload" then

		if Application.UserProperties("EnableHorizon")="Y" then
			ThisForm.Pages("PAGE1").Controls("btnMarkHorizon").enabled=true
			ThisForm.Pages("PAGE1").Controls("btnUpdateHorizon").enabled=true
			ThisForm.Pages("PAGE1").Controls("btnUseLastHorizon").enabled=true
		else
			ThisForm.Pages("PAGE1").Controls("btnMarkHorizon").enabled=false
			ThisForm.Pages("PAGE1").Controls("btnUpdateHorizon").enabled=false
			ThisForm.Pages("PAGE1").Controls("btnUseLastHorizon").enabled=false
		end if

		if Application.UserProperties("Paused")="true" then

			Application.UserProperties("Paused")="false"

		else

			'if Application.UserProperties("AnimalBookmark")="" then
			Application.UserProperties("AnimalBookmark")=""
			SFManager.ClearForm
			'set focus field to the first field
			Application.UserProperties("CurrentFocusField")=0

		end if

		'reopen to last editing sighting after a crash that occured offsegment
		if not AnimalBKPerOffSegment(0)="" and not Application.UserProperties("FormOpenMode")="Edit"  then

			Application.UserProperties("AnimalBookmark")= _
				AnimalBKPerOffSegment(UBound(AnimalBKPerOffSegment))
			SFManager.ClearForm
			SFManager.LoadFormFromRecord Application.UserProperties("AnimalBookmark")

		end if

		SetFormEditMode ThisFormName,true

		'load the sighting to edit if we are in edit mode
		if Application.UserProperties("FormOpenMode")="Edit" then
			SFManager.ClearForm

			SetFormEditMode ThisFormName,false

			Application.UserProperties("AnimalBookmark")= _
						Application.UserProperties("EditAnimalBookmark")

			SFManager.LoadFormFromRecord Application.UserProperties("AnimalBookmark")

		end if

		ThisForm.Pages("PAGE1").Controls("txtTransectID").Value=Application.UserProperties("TransectID")

	end if

    '===========================================================
    '===========================================================
	if SenderName="btnCloseEdit" then
		SFManager.UpdateRecord Application.UserProperties("AnimalBookmark")
		ErrorMessage=ValidateSheepFields(AnimalBKPerOffSegment)
		if not ErrorMessage="" then
			msgbox ErrorMessage
		else
			Application.UserProperties("EditAnimalBookmark")=""
			ThisForm.Close
		end if
	end if

	'===========================================================
	'INACTIVE - buttons were removed on client request
	'called when the up or down arrows are clicked. this function will
	'change the focus of the editable fields to the next field
	'===========================================================
	if SenderName="btnUp" or SenderName="btnDown" then

		'get the current focus field index
		FocusIndex=Application.UserProperties("CurrentFocusField")
		
		'if the user clicked up, move to the previous focus field or the bottom one if
		'if we are already at the top
		if SenderName="btnUp" then
			if FocusIndex=0 then
				FocusIndex=5
			else
				FocusIndex=FocusIndex-1
			end if
		end if

		'if the user clicked down, move to the next focus field or the top one if
		'if we are already at the bottom
		if SenderName="btnDown" then
			if FocusIndex=5 then
				FocusIndex=0
			else
				FocusIndex=FocusIndex+1
			end if
		end if

		'set the focus on the new current field and set it as the new current index
		ThisForm.Pages("PAGE1").Controls(ControlNameArray(FocusIndex)).SetFocus		
		Application.UserProperties("CurrentFocusField")=FocusIndex

	end if

    '===========================================================
	'this block is called when a calculator button is clicked
	'the value will be appended to the current value of the current
	'field in focus
	'===========================================================
	if IsNumeric(CalValue)=true or  SenderName="btnClear" then
		
		'get the current focus field
		FocusIndex=Application.UserProperties("CurrentFocusField")
		set FocusField=ThisForm.Pages("PAGE1").Controls(ControlNameArray(FocusIndex))

		'if we have a number, add it to the field
		if IsNumeric(CalValue)=true then
			
			if not IsNumeric(Application.UserProperties("LastSender")) then
				'if FocusField.Value="0" then FocusField.Value=""
				FocusField.Value=CalValue
			else
				'if FocusField.Value="0" then FocusField.Value=""
				FocusField.Value=FocusField.Value & CalValue
			end if

		end if

		'if we have a request the clear the field, clear it's contents
		if SenderName="btnClear" then
			FocusField.Value="0"
		end if

	end if

	'===========================================================
	'add a new sheep record to the database
	'===========================================================
	if SenderName="btnMarkAnimal" then

		'make sure we have a good gps connection
		ErrorMessage=MyGPS.LoadGPSValues()
		if not ErrorMessage="" then
			msgbox ErrorMessage
			exit sub
		end if

		'if we have a current record displaying, save that record's data before loading a new record
		if not Application.UserProperties("AnimalBookmark")="" then
			SFManager.UpdateRecord Application.UserProperties("AnimalBookmark")
			SFManager.ClearForm
		end if
		
		'get the globals that must be associated with each new record
		TransectID=Application.UserProperties("TransectID")
		SegmentID=CLng(Application.UserProperties("SegmentID"))
		SurveyID=Application.UserProperties("SurveyID")
		AnimalID=Application.UserProperties("AnimalID")	

		
		'insert a new blank record
		NewBookmark=SFManager.NewRecord(SurveyID,TransectID,SegmentID,AnimalID)

		if not IsNumeric(NewBookmark) then
			msgbox NewBookmark
		end if

		if IsNumeric(NewBookmark) then
		

			'set new animal id
			ThisForm.Pages("PAGE1").Controls("txtGroupID").Value=CStr(AnimalID)

			'increment the animal ID for the next animal to be created
			AnimalID=AnimalID+1
			Application.UserProperties("AnimalID")=AnimalID

			'set the new record as the current record
			Application.UserProperties("AnimalBookmark")=CStr(NewBookmark)

			'add the new records bookmark to our collection of bookmarks for this off-transect segment
			if AnimalBKPerOffSegment(0)="" then
				AnimalBKPerOffSegment(UBound(AnimalBKPerOffSegment))=CStr(NewBookmark)
			else
				redim preserve AnimalBKPerOffSegment(UBound(AnimalBKPerOffSegment)+1)
				AnimalBKPerOffSegment(UBound(AnimalBKPerOffSegment))=CStr(NewBookmark)
			end if

			'refresh map to show new inserted sighting
			Map.Redraw true

			'set focus field to the first field
			Application.UserProperties("CurrentFocusField")=0
			'ThisForm.Pages("PAGE1").Controls(ControlNameArray(0)).SetFocus

		end if

	end if

	'===========================================================
	'INACTIVE - button was removed on client request
	'===========================================================
	if SenderName="btnReloadLast" then

		
		AllOK=true

		
		if AnimalBKPerOffSegment(0)="" then
			msgbox "There are no previous sheep sightings."
			AllOK=false
		end if

		if AllOK=true then
			if UBound(AnimalBKPerOffSegment) < 1 then
				AllOK=false
			end if
		end if


		if AllOK=true then
			PrevAnimalBK=AnimalBKPerOffSegment(UBound(AnimalBKPerOffSegment)-1)
			CurAnimalID=Applet.Forms("frmSheepForm").Pages("PAGE1").Controls("txtGroupID").Value
			SFManager.LoadFormFromRecord PrevAnimalBK
			Applet.Forms("frmSheepForm").Pages("PAGE1").Controls("txtGroupID").Value=CurAnimalID
		end if
		

	end if

	'===========================================================
	'make the previous or next sighting the current one
	'===========================================================
	if SenderName="btnPrev" or SenderName="btnNext" then

		GoToAnimalBK=""
		CanNav=true

		'make sure we have sightings in our sightings array
		if AnimalBKPerOffSegment(0)="" then		
			msgbox "There are no animal sightings to navigate through."
			CanNav=false
		end if

		if CanNav=true then
			'get the currently displaying animal sighting and save it
			CurrentAnimalBK=Application.UserProperties("AnimalBookmark")
			SFManager.UpdateRecord CurrentAnimalBK
			
			'find the index of our current animal sighting in our sighting array
			Index=FindIndexByText(AnimalBKPerOffSegment,CurrentAnimalBK)
		end if

		'if we are not at the begining of the array, get the index of the sighting
		'before the current one
		if SenderName="btnPrev" and Index > 0 then
			GoToAnimalBK=AnimalBKPerOffSegment(Index-1)				
		end if

		'if we are not at the end of the array, get the sight just after the current one
		if SenderName="btnNext" and Index < UBound(AnimalBKPerOffSegment) then
			GoToAnimalBK=AnimalBKPerOffSegment(Index+1)
		end if

		'if we have an animal sighting to navigate to, load its data and make 
		'it our current sighting
		if not GoToAnimalBK="" then

			Application.UserProperties("AnimalBookmark")=GoToAnimalBK
			
			if Application.UserProperties("FormOpenMode")="Edit" then
		
				Dim CurAnimalFormName,NextAnimalFormName

				CurAnimalFormName=GetFeatureValue("animals",CurrentAnimalBK,"FORMNAME")
				NextAnimalFormName=GetFeatureValue("animals",GoToAnimalBK,"FORMNAME")

				if CurAnimalFormName<>NextAnimalFormName then
					ThisForm.Close
					CanNav=false
				end if

			end if

			if CanNav=true then
				SFManager.ClearForm
				SFManager.LoadFormFromRecord GoToAnimalBK
				'set focus field to the first field
				Application.UserProperties("CurrentFocusField")=0
				'ThisForm.Pages("PAGE1").Controls(ControlNameArray(0)).SetFocus	

			end if

		end if

	end if

	'===========================================================
	'update the current sigting position
	'===========================================================
	if SenderName="btnUpdatePosition" then
		if not Application.UserProperties("AnimalBookmark")="" then
			SFManager.UpdateRecordPosition Application.UserProperties("AnimalBookmark")
		end if
	end if

	'===========================================================
	'create a horizon record and point the current sighting to it
	'===========================================================
	if SenderName="btnMarkHorizon" then
		call btnMarkHorizon_Click
	end if

	'===========================================================
	'update the position of the horizon to the current gps position
	'===========================================================
	if SenderName="btnUpdateHorizon" then
		call btnUpdateHorizon_Clicked
	end if

	'===========================================================
	'get the last horizon created and point the current sighting to it
	'===========================================================
	if SenderName="btnUseLastHorizon" then
		call btnUseLastHorizon_Clicked
	end if

	'===========================================================
	'INACTIVE - temporarily hide the form without ending the offtransect session
	'===========================================================
	if SenderName="btnPause" then

		
		Application.UserProperties("Paused")="true"


		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnEndTransect").Visible=false
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotRight").Enabled=false
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnPilotLeft").Enabled=false
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnMarkAnimal").Enabled=true
		Application.Toolbars("tlbNPSTransToolbar").Item("tlbtnEndTransect").Enabled=true


		ThisForm.Close	

	end if

	'===========================================================
	'called by all of the numeric editable fields when their values change
	'this block recalculates the total and sets the value to the total field
	'===========================================================
	if EventName="onchange" then

		AllOK=false
		AnimalSum=0

		'if this is the long form, there are more fields to add up
		Total=5
		if ThisFormName = "frmSheepFormLong" then Total=11

		'see if sender is one of our fields that add to the total
		for Index=0 to Total
			if ControlNameArray(Index)=SenderName then
				AllOK=true
			end if
		next
	
		if AllOK=true then

			'add up all the values in our data fields
			for Index=0 to Total

				ChangeFieldValue=ThisForm.Pages("PAGE1").Controls(ControlNameArray(Index)).Value

				if IsNumeric(ChangeFieldValue)=true then
					AnimalSum=AnimalSum+CDbl(ChangeFieldValue)
				end if

			next

			'set the total as the sum of the values in all our data fields
			ThisForm.Pages("PAGE1").Controls("txtTotal").Value =CStr(AnimalSum)

		end if
		
	end if

	'===========================================================
	'===========================================================
	if EventName="onfocus" then
			

		AllOK=false

		'if this is the long form, there are more fields to add up
		Total=5
		if ThisFormName = "frmSheepFormLong" then Total=11

		'see if sender is one of our fields that add to the total
		for Index=0 to Total
			if ControlNameArray(Index)=SenderName then
				AllOK=true
			end if
		next

		if AllOK=true then

			'change the global index of the editing field
			for Index=0 to Total
				if SenderName=ControlNameArray(Index) then

					Application.UserProperties("CurrentFocusField")=Index

					Application.UserProperties("CurrentFocusFieldValue")= _
						ThisForm.Pages("PAGE1").Controls(SenderName).Value

					ThisForm.Pages("PAGE1").Controls(SenderName).Value=""

				end if
			next

		end if

	end if


	'===========================================================
	'===========================================================
	if EventName="onblur" then

		AllOK=false

		'if this is the long form, there are more fields to add up
		Total=5
		if ThisFormName = "frmSheepFormLong" then Total=11

		'see if sender is one of our fields that add to the total
		for Index=0 to Total
			if ControlNameArray(Index)=SenderName then
				AllOK=true
			end if
		next

		if AllOK=true then

			for Index=0 to Total
				if SenderName=ControlNameArray(Index) then

					if IsNumeric(ThisForm.Pages("PAGE1").Controls(SenderName).Value) then
						ChangeFieldValue=CDbl(ThisForm.Pages("PAGE1").Controls(SenderName).Value)
						ThisForm.Pages("PAGE1").Controls(SenderName).Value=CStr(ChangeFieldValue)
					else
						if Application.UserProperties("CurrentFocusFieldValue")="" then
							ThisForm.Pages("PAGE1").Controls(SenderName).Value="0"
						else
							ThisForm.Pages("PAGE1").Controls(SenderName).Value= _
							Application.UserProperties("CurrentFocusFieldValue")
						end if
					end if

				end if
			next

		end if

	end if
	

	'===========================================================
	'reopen the hidden sheep form
	'===========================================================
	if SenderName="btnResumeTransect" then

		'if we have a current record displaying, save that record's data before resuming transect
		if not Application.UserProperties("AnimalBookmark")="" then
			SFManager.UpdateRecord CLng(Application.UserProperties("AnimalBookmark"))
			
		end if

		ErrorMessage=ValidateSheepFields(AnimalBKPerOffSegment)
		if not ErrorMessage="" then
			msgbox ErrorMessage
		else

			SFManager.ClearForm
	
			'close the form
			ThisForm.Close

			'end off-transect segment and reset necessary globals
			call ResumeTransect
		
		end if


	end if

	'set transect number
	if ThisForm.Pages("PAGE1").Controls("txtTransectID").Value="" then
		ThisForm.Pages("PAGE1").Controls("txtTransectID").Value= _
				Application.UserProperties("TransectID")
	end if

	Application.UserProperties("LastSender")=CalValue


	'release all objects that might have been used
	set ThisForm=nothing
	set FocusField=nothing
	set SFManager=nothing

end sub

'============================================================================================
'INACTIVE=(replaced by frmWeather_Event())-collects weather info when end transect is clicked
'============================================================================================
sub frmEndTransect_Event(EventName)

	dim ThisForm,myRS,TransLayer,TransectID

	TransectID=Application.UserProperties("TransectID")


	set ThisForm=Applet.Forms("frmEndTransect")

	if EventName="ok" then
		
		set TransLayer=Application.Map.Layers("TrnOrig.shp") 

		if not TransLayer is nothing then

			call RemoveAllLayersFromEditMode
			TransLayer.Editable=true

			set myRS=TransLayer.Records
			myRS.Bookmark=myRS.Find("[TransectID]=" & TransectID) 

			if not myRS.Fields("WEATHER") is nothing then
				myRS.Fields("WEATHER").Value=ThisForm.Pages("PAGE1").Controls("cboWeather").value
			end if
			if not myRS.Fields("CLOUDCOVER") is nothing then
				myRS.Fields("CLOUDCOVER").Value=ThisForm.Pages("PAGE1").Controls("cboCloudCover").value
			end if
			if not myRS.Fields("PRECIP") is nothing then
				myRS.Fields("PRECIP").Value=ThisForm.Pages("PAGE1").Controls("cboPrecipitation").value
			end if
			if not myRS.Fields("TURBINT") is nothing then
				myRS.Fields("TURBINT").Value=ThisForm.Pages("PAGE1").Controls("cboTurbulenceIntensity").value
			end if
			if not myRS.Fields("TURBDUR") is nothing then
				myRS.Fields("TURBDUR").Value=ThisForm.Pages("PAGE1").Controls("cboTurbulenceDuration").value
			end if

			myRS.Update
			
		end if

		TransLayer.Editable=false
		ThisForm.Close
		
	end if

	

	set myRS=nothing
	set ThisForm=nothing

end sub

'============================================================================================
'============================================================================================
sub frmAlertEndTransect_Event(SenderName,EventName)

	Dim ThisPage

	set ThisPage=Applet.Forms("frmEndTransect").Pages("PAGE1")

	if SenderName="btnOK" then
		Applet.Forms("frmAlertEndTransect").Close
	end if

	if SenderName="btnEndTransect" then
		Applet.Forms("frmAlertEndTransect").Close
		Call tlbtnEndTransect_Click
	end if

	set ThisPage=nothing

end sub

'===========================================================
'-collects weather info when end transect is clicked
'===========================================================
sub frmWeather_Event(SenderName,EventName)

	Dim ThisForm,ThisList,CurControlName
	Dim RowTotal,ColumnTotal,RowIndex,ColumnIndex
	Dim DBPath,DBFieldNamesArray(4)
	Dim IndexButton
	dim myRS,TransLayer,CurVal,TransectID,CurrentValue


	TransectID=Application.UserProperties("TransectID")
	DBFieldNamesArray(0)="CLOUDCOVER"
	DBFieldNamesArray(1)="PRECIP"
	DBFieldNamesArray(2)="TURBINT"
	DBFieldNamesArray(3)="TURBDUR"
	DBPath=Application.Path & "\Applets\Survey\defaultvalues.dbf"

	set ThisForm=Applet.Forms("frmWeather")

	if Len(SenderName)>3 then
		IndexButton=Mid(SenderName,4)
	end if

	'===========================================================
	'===========================================================
	if EventName="onload" then
	
		'clear any prev values
		ThisForm.Pages("PAGE1").Controls("txtTemperature").Value=""

		set TransLayer=Application.Map.Layers("TrnOrig.shp") 
		set myRS=TransLayer.Records
		myRS.Bookmark=myRS.Find("[TransectID]=" & TransectID) 

		if not myRS.Fields("TEMPRTURE") is nothing then
			ThisForm.Pages("PAGE1").Controls("txtTemperature").Value= _
				myRS.Fields("TEMPRTURE").Value
		end if
		

		RowTotal=3
		for RowIndex=0 to RowTotal
			
			ThisList=GetRecordsAsArray(DBPath,DBFieldNamesArray(RowIndex))

			CurrentValue=""
			if not myRS.Fields(DBFieldNamesArray(RowIndex)) is nothing then
				CurrentValue=myRS.Fields(DBFieldNamesArray(RowIndex)).Value
			end if
	
			if IsArray(ThisList)=true then

				ColumnTotal=UBound(ThisList)

				for ColumnIndex=0 to 3

					CurControlName="btn" & CStr(RowIndex) &  CStr(ColumnIndex) 

					if ColumnIndex < ColumnTotal+1 then
				
						ThisForm.Pages("PAGE1").Controls(CurControlName).Visible=true
						ThisForm.Pages("PAGE1").Controls(CurControlName).Text=ThisList(ColumnIndex)

						if CurrentValue=ThisList(ColumnIndex) then
							SelControlName="btn" & RowIndex & ColumnIndex
							SetControlSelected "frmWeather","PAGE1",SelControlName,""	
						end if		

					else
						ThisForm.Pages("PAGE1").Controls(CurControlName).Visible=false
					end if

				next
			else
				for ColumnIndex=0 to 3

					CurControlName="btn" & CStr(RowIndex) &  CStr(ColumnIndex) 
					ThisForm.Pages("PAGE1").Controls(CurControlName).Visible=false

				next
			end if
		next
		
	end if

	'===========================================================
	'one of the options was selected
	'===========================================================
	if IsNumeric(IndexButton) then
		Dim SelRow,SelColumn,SelControlName

		SelRow=Left(IndexButton,1)
		SelColumn=Right(IndexButton,1)
		SelControlName="btn" & SelRow & SelColumn
		
		for ColumnIndex=0 to 3
			CurControlName="btn" & SelRow & CStr(ColumnIndex)
			if ThisForm.Pages("PAGE1").Controls(CurControlName).Visible=true then
				SetControlNotSelected "frmWeather","PAGE1",CurControlName
			end if
		next

		SetControlSelected "frmWeather","PAGE1",SelControlName,""

	end if

	'===========================================================
	'===========================================================
	if SenderName="btnEasyNumbers" then
		
		CurVal=ThisForm.Pages("PAGE1").Controls("txtTemperature").Value
		CurVal=GetEasyNumber(CurVal)
		if not CurVal="cancel" then
			ThisForm.Pages("PAGE1").Controls("txtTemperature").Value=CurVal
		end if
	end if


	'===========================================================
	'===========================================================
	if SenderName="btnOK" then

		set TransLayer=Application.Map.Layers("TrnOrig.shp") 

		if not TransLayer is nothing then

			call RemoveAllLayersFromEditMode
			TransLayer.Editable=true

			set myRS=TransLayer.Records
			myRS.Bookmark=myRS.Find("[TransectID]=" & TransectID) 
			
			if not myRS.Fields("CLOUDCOVER") is nothing then
				CurVal=""
				RowIndex=0
				for ColumnIndex=0 to 3
					CurControlName="btn" & CStr(RowIndex) & CStr(ColumnIndex)
					if ThisForm.Pages("PAGE1").Controls(CurControlName).Visible=true _
						and Mid(ThisForm.Pages("PAGE1").Controls(CurControlName).Text,1,1)="*"  then
						SetControlNotSelected "frmWeather","PAGE1",CurControlName
						CurVal=ThisForm.Pages("PAGE1").Controls(CurControlName).Text
					end if
				next
				myRS.Fields("CLOUDCOVER").Value=CurVal
			end if
			if not myRS.Fields("PRECIP") is nothing then
				CurVal=""
				RowIndex=1
				for ColumnIndex=0 to 3
					CurControlName="btn" & CStr(RowIndex) & CStr(ColumnIndex)
					if ThisForm.Pages("PAGE1").Controls(CurControlName).Visible=true _
						and Mid(ThisForm.Pages("PAGE1").Controls(CurControlName).Text,1,1)="*"  then
						SetControlNotSelected "frmWeather","PAGE1",CurControlName
						CurVal=ThisForm.Pages("PAGE1").Controls(CurControlName).Text
					end if
				next
				myRS.Fields("PRECIP").Value=CurVal
			end if
			if not myRS.Fields("TURBINT") is nothing then
				CurVal=""
				RowIndex=2
				for ColumnIndex=0 to 3
					CurControlName="btn" & CStr(RowIndex) & CStr(ColumnIndex)
					if ThisForm.Pages("PAGE1").Controls(CurControlName).Visible=true _
						and Mid(ThisForm.Pages("PAGE1").Controls(CurControlName).Text,1,1)="*"  then
						SetControlNotSelected "frmWeather","PAGE1",CurControlName
						CurVal=ThisForm.Pages("PAGE1").Controls(CurControlName).Text
					end if
				next
				myRS.Fields("TURBINT").Value=CurVal
			end if
			if not myRS.Fields("TURBDUR") is nothing then
				CurVal=""
				RowIndex=3
				for ColumnIndex=0 to 3
					CurControlName="btn" & CStr(RowIndex) & CStr(ColumnIndex)
					if ThisForm.Pages("PAGE1").Controls(CurControlName).Visible=true _
						and Mid(ThisForm.Pages("PAGE1").Controls(CurControlName).Text,1,1)="*"  then
						SetControlNotSelected "frmWeather","PAGE1",CurControlName
						CurVal=ThisForm.Pages("PAGE1").Controls(CurControlName).Text
					end if
				next
				myRS.Fields("TURBDUR").Value=CurVal
			end if
			if not myRS.Fields("TEMPRTURE") is nothing then
				myRS.Fields("TEMPRTURE").Value=ThisForm.Pages("PAGE1").Controls("txtTemperature").Value
			end if

			myRS.Update
			
			TransLayer.Editable=false
		end if

		
		ThisForm.Close
		
	end if


	set TransLayer=nothing
	set myRS=nothing
	set ThisForm=nothing

end sub

'===========================================================
'===========================================================
sub tlbtnEditAutPan_Click

	if GPS.IsOpen = false then
		msgbox "GPS is not active"
		exit sub
	end if
	Applet.Forms("frmEditAutoPan").Show
end sub

'===========================================================
'===========================================================
sub frmEditAutoPan_Event(SenderName,EventName)

	Dim ThisForm
	Dim CurVal


	set ThisForm=Applet.Forms("frmEditAutoPan")

	'===========================================================
	'===========================================================
	if EventName = "onload" then
		ThisForm.Pages("PAGE1").Controls("txtPecExtent").Value=CStr(GPS.AutoPanMargin)
	end if

	'===========================================================
	'===========================================================
	if SenderName = "btnUp" then

		CurVal=ThisForm.Pages("PAGE1").Controls("txtPecExtent").Value
		if IsNumeric(CurVal)=false then
			CurVal=0.0
		end if

		CurVal=CurVal+0.1
		if CurVal > 1 then
			CurVal=1
		end if

		ThisForm.Pages("PAGE1").Controls("txtPecExtent").Value=CStr(CurVal)
	end if

	'===========================================================
	'===========================================================
	if SenderName = "btnDown" then

		CurVal=ThisForm.Pages("PAGE1").Controls("txtPecExtent").Value
		if IsNumeric(CurVal)=false then
			CurVal=0.0
		end if

		CurVal=CurVal-0.1
		if CurVal < 0 then
			CurVal=0.0
		end if

		ThisForm.Pages("PAGE1").Controls("txtPecExtent").Value=CStr(CurVal)
	end if

	'===========================================================
	'===========================================================
	if SenderName = "btnOK" then

		CurVal=ThisForm.Pages("PAGE1").Controls("txtPecExtent").Value
		if IsNumeric(CurVal)=false then
			CurVal=0.5
		end if

		GPS.AutoPanMargin=CurVal

		ThisForm.Close
	end if

	'===========================================================
	'===========================================================
	if SenderName = "btnCancel" then
		ThisForm.Close
	end if

	set ThisForm=nothing


end sub

'===========================================================
'===========================================================
class SheepFormManager

	private m_FormName

	public sub Init(FormName)
		m_FormName=FormName
	end sub

	'====================================================
	public sub ClearForm

		Dim ThisForm

		set ThisForm=Applet.Forms(m_FormName)	

		'clear form 
		ThisForm.Pages("PAGE1").Controls("txtEweLike").Value="0"
		ThisForm.Pages("PAGE1").Controls("txtLambs").Value="0"
		ThisForm.Pages("PAGE1").Controls("txtLT_FullCurlRam").Value="0"
		ThisForm.Pages("PAGE1").Controls("txtGT_FullCurlRam").Value="0"
		ThisForm.Pages("PAGE1").Controls("txtUnclassifiedRams").Value ="0"
		ThisForm.Pages("PAGE1").Controls("txtUnclassifiedSheep").Value ="0"

		if ThisForm.Name = "frmSheepFormLong" then
	    	ThisForm.Pages("PAGE1").Controls("txtEwes").Value ="0"
	    	ThisForm.Pages("PAGE1").Controls("txtYrlgs").Value ="0"
	    	ThisForm.Pages("PAGE1").Controls("txtRams_LTHalf").Value ="0"
	    	ThisForm.Pages("PAGE1").Controls("txtRams_Half").Value ="0"
	    	ThisForm.Pages("PAGE1").Controls("txtRams_3_qtrs").Value ="0"
	    	ThisForm.Pages("PAGE1").Controls("txt_7_Eighths").Value ="0"
		end if
	
		PopulateComboBox DefaultValuesPath, ThisForm.Pages("PAGE1").Controls("lstActivity"),"ACTIVITY"


		'readonly to user
		ThisForm.Pages("PAGE1").Controls("txtTotal").Value ="0"
		ThisForm.Pages("PAGE1").Controls("txtGroupID").Value =""
		ThisForm.Pages("PAGE1").Controls("txtTransectID").Value =""
		
		set ThisForm=nothing

	end sub

	'====================================================
	public function NewRecord(SurveyID,TransectID,SegmentID,AnimalID)
		NewRecord=SetSpatialData(SurveyID,TransectID,SegmentID,AnimalID,-1,"insert")
	end function

	'====================================================
	public function UpdateRecordPosition(AnimalBookmark)
		UpdateRecordPosition=SetSpatialData(-1,-1,-1,-1,AnimalBookmark,"update")
	end function

	'====================================================
	public function SetSpatialData(SurveyID,TransectID,SegmentID,AnimalID, _
		AnimalBookmark,Action)

		Dim ErrorMessage,ThisPoint,Altitude,Latitude,Longitude
		Dim lyrAnimal,PDOP,SOG,ThisForm,myRS

	

		'check if we have everything set for an insert
		ErrorMessage=ReqCheck()
		if not ErrorMessage="" then
			SetSpatialData=ErrorMessage
			exit function
		end if


		'get animal layer
		set lyrAnimal = Application.Map.Layers("animals")

		'move layer into edit mode
		lyrAnimal.Editable=true

		'create a point
		set ThisPoint=Application.CreateAppObject ("Point")
		ThisPoint.X=GPS.X
		ThisPoint.Y=GPS.Y
		ThisPoint.Z=GPS.Z
		Altitude=GPS.Altitude
		Latitude=GPS.Latitude
		Longitude=GPS.Longitude
		PDOP=GPS.Properties("PDOP")
		SOG=GPS.Properties("SOG")

		if Action="insert" then

			'create new record with point
			set myRS=lyrAnimal.Records
			myRS.AddNew ThisPoint

		else
			'load existing record and set new point
			set myRS=lyrAnimal.Records
			myRS.Bookmark=CLng(AnimalBookmark)
			set myRS.Fields.Shape=ThisPoint
		end if


		'get teh form
		set ThisForm=Applet.Forms(m_FormName)	

		if Action="insert" then

			'add in all the calculated values
			if not myRS.Fields("SurveyName") is nothing then
				myRS.Fields("SurveyName").Value=Application.UserProperties("SurveyName")
			end if
			if not myRS.Fields("FORMNAME") is nothing then
				myRS.Fields("FORMNAME").Value=Application.UserProperties("AnimalForm")
			end if
			if not myRS.Fields("Park") is nothing then
				myRS.Fields("Park").Value=Application.UserProperties("Park")
			end if
			if not myRS.Fields("Year") is nothing then
				myRS.Fields("Year").Value=GetCurrentYear()
			end if
			if not myRS.Fields("TransectID") is nothing then
				myRS.Fields("TransectID").Value=TransectID
			end if
			if not myRS.Fields("SegmentID") is nothing then
				myRS.Fields("SegmentID").Value=SegmentID
			end if
			if not myRS.Fields("SurveyID") is nothing then
				myRS.Fields("SurveyID").Value=SurveyID
			end if
			if not myRS.Fields("AnimalID") is nothing then
				myRS.Fields("AnimalID").Value=AnimalID
				ThisForm.Pages("PAGE1").Controls("txtGroupID").Value=CStr(AnimalID)
			end if
			if not myRS.Fields("ANIMALTYPE") is nothing then
				myRS.Fields("ANIMALTYPE").Value="SHEEP"
			end if
			if not myRS.Fields("DATE_") is nothing then
				myRS.Fields("DATE_").Value=GetCurrentDateShort()
			end if
			if not myRS.Fields("TIME_") is nothing then
				myRS.Fields("TIME_").Value=GetCurrentTime()
			end if

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
			myRS.Fields("PDOP").Value=PDOP
		end if
		if not myRS.Fields("PLANESPD") is nothing then
			myRS.Fields("PLANESPD").Value=SOG
		end if
		if not myRS.Fields("DIST2TRANS") is nothing then
			myRS.Fields("DIST2TRANS").Value=DistanceFromLastSegment(ThisPoint)
		end if
		
		myRS.Update

		SetSpatialData=myRS.Bookmark


		lyrAnimal.Editable=false

		set ThisForm=nothing
		set lyrAnimal =nothing
		set ThisPoint=nothing
		set myRS=nothing

	end function

	'====================================================
	public function LoadFormFromRecord(AnimalBookmark)

		dim lyrAnimal,myRS,RecVal,ThisForm,ErrorMessage



		'check if we have everything set for an insert
		ErrorMessage=ReqCheck()
		if not ErrorMessage="" then
			LoadFormFromRecord=ErrorMessage
			exit function
		end if

		'get animal layer
		set lyrAnimal = Application.Map.Layers("animals")


		'move to animal record
		set myRS=lyrAnimal.Records
		myRS.Bookmark=CLng(AnimalBookmark)

		'get animal form
		set ThisForm=Applet.Forms(m_FormName)

		'set all form values		
		if not myRS.Fields("EWELIKE") is nothing then
			RecVal=myRS.Fields("EWELIKE")
			if not IsNull(RecVal) and IsNumeric(RecVal) then
				ThisForm.Pages("PAGE1").Controls("txtEweLike").Value=RecVal
			end if
		end if
		if not myRS.Fields("LAMBS") is nothing then
			RecVal=myRS.Fields("LAMBS")
			if not IsNull(RecVal) and IsNumeric(RecVal) then
				ThisForm.Pages("PAGE1").Controls("txtLambs").Value=RecVal
			end if
		end if
		if not myRS.Fields("LT_FCRAMS") is nothing then
			RecVal=myRS.Fields("LT_FCRAMS")
			if not IsNull(RecVal) and IsNumeric(RecVal) then
				ThisForm.Pages("PAGE1").Controls("txtLT_FullCurlRam").Value=RecVal
			end if
		end if
		if not myRS.Fields("GTE_FCRAMS") is nothing then
			RecVal=myRS.Fields("GTE_FCRAMS")
			if not IsNull(RecVal) and IsNumeric(RecVal) then
				ThisForm.Pages("PAGE1").Controls("txtGT_FullCurlRam").Value=RecVal
			end if
		end if
		if not myRS.Fields("UNCLSSRAMS") is nothing then
			RecVal=myRS.Fields("UNCLSSRAMS")
			if not IsNull(RecVal) and IsNumeric(RecVal) then
				ThisForm.Pages("PAGE1").Controls("txtUnclassifiedRams").Value=RecVal
			end if
		end if
		if not myRS.Fields("UNCLSSHEEP") is nothing then
			RecVal=myRS.Fields("UNCLSSHEEP")
			if not IsNull(RecVal) and IsNumeric(RecVal) then
				ThisForm.Pages("PAGE1").Controls("txtUnclassifiedSheep").Value=RecVal
			end if
		end if
		if not myRS.Fields("TOTAL") is nothing then
			RecVal=myRS.Fields("TOTAL")
			if not IsNull(RecVal) and IsNumeric(RecVal) then
				ThisForm.Pages("PAGE1").Controls("txtTotal").Value=RecVal
			end if
		end if

		if ThisForm.Name = "frmSheepFormLong" then

			if not myRS.Fields("EWES") is nothing then
				RecVal=myRS.Fields("EWES")
				if not IsNull(RecVal) and IsNumeric(RecVal) then
					ThisForm.Pages("PAGE1").Controls("txtEwes").Value=RecVal
				end if
			end if
			if not myRS.Fields("YEARLING") is nothing then
				RecVal=myRS.Fields("YEARLING")
				if not IsNull(RecVal) and IsNumeric(RecVal) then
					ThisForm.Pages("PAGE1").Controls("txtYrlgs").Value=RecVal
				end if
			end if
			if not myRS.Fields("LT_1_2CURL") is nothing then
				RecVal=myRS.Fields("LT_1_2CURL")
				if not IsNull(RecVal) and IsNumeric(RecVal) then
					ThisForm.Pages("PAGE1").Controls("txtRams_LTHalf").Value=RecVal
				end if
			end if
			if not myRS.Fields("CURL_1_2") is nothing then
				RecVal=myRS.Fields("CURL_1_2")
				if not IsNull(RecVal) and IsNumeric(RecVal) then
					ThisForm.Pages("PAGE1").Controls("txtRams_Half").Value=RecVal
				end if
			end if
			if not myRS.Fields("CURL_3_4") is nothing then
				RecVal=myRS.Fields("CURL_3_4")
				if not IsNull(RecVal) and IsNumeric(RecVal) then
					ThisForm.Pages("PAGE1").Controls("txtRams_3_qtrs").Value=RecVal
				end if
			end if
			if not myRS.Fields("CURL_7_8") is nothing then
				RecVal=myRS.Fields("CURL_7_8")
				if not IsNull(RecVal) and IsNumeric(RecVal) then
					ThisForm.Pages("PAGE1").Controls("txt_7_Eighths").Value=RecVal
				end if
			end if

		end if

		if not myRS.Fields("ACTIVITY") is nothing then
			if not myRS.Fields("ACTIVITY").IsNull then
				SetListIndexByItemText myRS.Fields("ACTIVITY").Value,ThisForm.Pages("PAGE1").Controls("lstActivity")
			end if
		end if
		if not myRS.Fields("AnimalID") is nothing then
			RecVal=myRS.Fields("AnimalID")
			if not IsNull(RecVal) and IsNumeric(RecVal) then
				ThisForm.Pages("PAGE1").Controls("txtGroupID").Value=RecVal
			end if
		end if
		if not myRS.Fields("TransectID") is nothing then
			RecVal=myRS.Fields("TransectID")
			if not IsNull(RecVal) and IsNumeric(RecVal) then
				ThisForm.Pages("PAGE1").Controls("txtTransectID").Value=RecVal
			end if
		end if


		set lyrAnimal=nothing
		set myRS=nothing
		set ThisForm=nothing

	end function

	'====================================================
	public function UpdateRecord(AnimalBookmark)

		dim lyrAnimal,myRS,RecVal,ThisForm,ErrorMessage




		'check if we have everything set for an insert
		ErrorMessage=ReqCheck()
		if not ErrorMessage="" then
			UpdateRecord=ErrorMessage
			exit function
		end if

		'get animal layer
		set lyrAnimal = Application.Map.Layers("animals")


		'move layer into edit mode
		lyrAnimal.Editable=true

		'move to animal record
		set myRS=lyrAnimal.Records
		myRS.Bookmark=CLng(AnimalBookmark)

		'get animal form
		set ThisForm=Applet.Forms(m_FormName)

		if not myRS.Fields("EWELIKE") is nothing then
			RecVal=ThisForm.Pages("PAGE1").Controls("txtEweLike").Value
			if IsNumeric(RecVal) then
				myRS.Fields("EWELIKE").Value=RecVal
			end if
		end if
		if not myRS.Fields("LAMBS") is nothing then
			RecVal=ThisForm.Pages("PAGE1").Controls("txtLambs").Value
			if IsNumeric(RecVal) then
				myRS.Fields("LAMBS").Value=RecVal
			end if
		end if
		if not myRS.Fields("LT_FCRAMS") is nothing then
			RecVal=ThisForm.Pages("PAGE1").Controls("txtLT_FullCurlRam").Value
			if IsNumeric(RecVal) then
				myRS.Fields("LT_FCRAMS").Value=RecVal
			end if
		end if
		if not myRS.Fields("GTE_FCRAMS") is nothing then
			RecVal=ThisForm.Pages("PAGE1").Controls("txtGT_FullCurlRam").Value
			if IsNumeric(RecVal) then
				myRS.Fields("GTE_FCRAMS").Value=RecVal
			end if
		end if
		if not myRS.Fields("UNCLSSRAMS") is nothing then
			RecVal=ThisForm.Pages("PAGE1").Controls("txtUnclassifiedRams").Value
			if IsNumeric(RecVal) then
				myRS.Fields("UNCLSSRAMS").Value=RecVal
			end if
		end if
		if not myRS.Fields("UNCLSSHEEP") is nothing then
			RecVal=ThisForm.Pages("PAGE1").Controls("txtUnclassifiedSheep").Value
			if IsNumeric(RecVal) then
				myRS.Fields("UNCLSSHEEP").Value=RecVal
			end if
		end if
		if not myRS.Fields("TOTAL") is nothing then
			RecVal=ThisForm.Pages("PAGE1").Controls("txtTotal").Value
			if IsNumeric(RecVal) then
				myRS.Fields("TOTAL").Value=RecVal
			end if
		end if
		if not myRS.Fields("PILOTLNAM") is nothing then
			myRS.Fields("PILOTLNAM").Value=Application.UserProperties("Pilot")
		end if
		if not myRS.Fields("PILOTDIR") is nothing then
			myRS.Fields("PILOTDIR").Value=Application.UserProperties("PilotDir")
		end if
		if not myRS.Fields("OBS1LNAM") is nothing then
			myRS.Fields("OBS1LNAM").Value=Application.UserProperties("Observer1")
		end if
		if not myRS.Fields("OBS1DIR") is nothing then
			myRS.Fields("OBS1DIR").Value=Application.UserProperties("ObsDir1")
		end if

		if not Application.UserProperties("Observer2")="" then

			if not myRS.Fields("OBS2LNAM") is nothing then
				myRS.Fields("OBS2LNAM").Value=Application.UserProperties("Observer2")
			end if
			if not myRS.Fields("OBS2DIR") is nothing then
				myRS.Fields("OBS2DIR").Value=Application.UserProperties("ObsDir2")
			end if

		end if

		if ThisForm.Name = "frmSheepFormLong" then

			if not myRS.Fields("EWES") is nothing then
				RecVal=ThisForm.Pages("PAGE1").Controls("txtEwes").Value
				if IsNumeric(RecVal) then
					myRS.Fields("EWES").Value=RecVal
				end if
			end if
			if not myRS.Fields("YEARLING") is nothing then
				RecVal=ThisForm.Pages("PAGE1").Controls("txtYrlgs").Value
				if IsNumeric(RecVal) then
					myRS.Fields("YEARLING").Value=RecVal
				end if
			end if
			if not myRS.Fields("LT_1_2CURL") is nothing then
				RecVal=ThisForm.Pages("PAGE1").Controls("txtRams_LTHalf").Value
				if IsNumeric(RecVal) then
					myRS.Fields("LT_1_2CURL").Value=RecVal
				end if
			end if
			if not myRS.Fields("CURL_1_2") is nothing then
				RecVal=ThisForm.Pages("PAGE1").Controls("txtRams_Half").Value
				if IsNumeric(RecVal) then
					myRS.Fields("CURL_1_2").Value=RecVal
				end if
			end if
			if not myRS.Fields("CURL_3_4") is nothing then
				RecVal=ThisForm.Pages("PAGE1").Controls("txtRams_3_qtrs").Value
				if IsNumeric(RecVal) then
					myRS.Fields("CURL_3_4").Value=RecVal
				end if
			end if
			if not myRS.Fields("CURL_7_8") is nothing then
				RecVal=ThisForm.Pages("PAGE1").Controls("txt_7_Eighths").Value
				if IsNumeric(RecVal) then
					myRS.Fields("CURL_7_8").Value=RecVal
				end if
			end if

		end if

		if not myRS.Fields("ACTIVITY") is nothing then
			RecVal=ThisForm.Pages("PAGE1").Controls("lstActivity").Text
			myRS.Fields("ACTIVITY").Value=RecVal
		end if

		myRS.Update

		lyrAnimal.Editable=false
		
		set lyrAnimal=nothing
		set myRS=nothing
		set ThisForm=nothing

	end function


	'====================================================
	private function ReqCheck()

		dim lyrAnimal, ErrorMessage

		if not Application.UserProperties("FormOpenMode")="Edit" then
			'check gps 
			ErrorMessage=CheckGPS()
			if not ErrorMessage="" then
				ReqCheck=ErrorMessage
				exit function
			end if

		end if

		'make sure the layer is in the map
		set lyrAnimal = Application.Map.Layers("animals")
		if lyrAnimal is nothing then
			ReqCheck="Could not find the Animal layer"
			exit function
		end if

		'move all layers out of edit mode
		call RemoveAllLayersFromEditMode

		'make sure the layer is editable
		if lyrAnimal.CanEdit=false then 
			set lyrAnimal=nothing
			ReqCheck="not editable"
			exit function
		end if

		ReqCheck=""
		
		set lyrAnimal=nothing

	end function

end class


'===========================================================
'this function is called by every form on the default page's onvalidate and onqueryclose
'events to prevent closing the form by clicking the ok or cancel button. It is also
'called when the form is closed programmatically but will not prevent the actual
'closing of the form when called this way. When closing the form programmatically,
'always call Form.Close with no arguments which will fire the event OnQueryCancel
'so that the ThisEvent.MessageText is not displayed in a popup. Closing the form with
'Form.Close(True) will cause ThisEvent.MessageText and ThisEvent.MessageType to become 
'active and will display a popup message
'===========================================================
sub PreventCloseByOkOrCancel
	ThisEvent.Result=false
	ThisEvent.MessageText ="This button is disabled."
	ThisEvent.MessageType=vbInformation
end sub

'===========================================================
'===========================================================
sub RemoveAllLayersFromEditMode

	dim LayerCount,LayerIndex,CurrentLayer
	
	LayerCount= Application.Map.Layers.Count

	for LayerIndex=1 to LayerCount
		Set CurrentLayer = Application.Map.Layers(LayerIndex)
		if CurrentLayer.CanEdit then
			CurrentLayer.Editable=false
		end if
	next

end sub
