# NPS Transect Generator

This is an ArcMap 10.x Addin for creating transects for sheep
surveys.  It was developed by GeoNorth for the Central Alaska
Network (Kumi Rattenbury) pre 2010. It was originally an ArcMap
9.3/10.0 extension, and ArcPad data collection applet.

In 2012, Kumi requested support from AKRO GIS when the extension
stopped working after an ArcMap upgrade. AKRO GIS agreed to
support the extension's functionality as an ArcMap Addin.
AKRO GIS never supported the ArcPad applet.

AKRO GIS created the `NPSTransectAddin` folder and code. It mostly
contains just the files needed to add Addin fuctionality.
The bulk of the code remains largely unchanged in the original
`NPSTranseectTool` folder. See the commit history for details.

## Build

Install the ArcObjects SDK (comes with ArcGIS Desktop 10.x) Open
the solution in the version Visual Studio supported by your version
of ArcGIS. Select `Build Solution` from the VS menu.

## Deploy

The original tool came to us with an obsolete (even then) MS
Setup configuration (in `NPSTransectToolSetup`), that was used
to create an `msi` installation file.  This no longer works,
and is not supported.  Instead the tool creates an esri Add-In
package.

After building a `release` version, copy the file
`NPSTransectAddIn/bin/release/NPSTransect.esriAddin` to
`X:\GIS\Addins\10.X` where it will automatically be
installed for all Alaska GIS users.  Users without network
access can get a copy from someone who does, and then
double click the addin file to launch the esri addin
install tool.

Users will also need a copy of the ArcMap document and
file geodatabase in `NPS_Transect_Map_and_Database.zip`

Note: `Docs/NPS_Transect_Tool_v2.0_installation.docx`
is for the extension and is not longer applicable.

## Using

1) Copy the ArcMap document and file geodatabase in
`NPS_Transect_Map_and_Database.zip` to a local folder.

2) Open the ArcMap document and edit the geodatabase
to suit the project requirements.  Do not change the
data schema, or layer properties.

3) In the `Customize -> Toolbars` menu option,
select the `NPS Transects`.

4) Explore the tools in the tool bar and use them to
fill out the remainder of the survey requirements and
create the survey transects and buffers.  I believe
the tools must be used from left to right with the
last tool being optional.
These tools will not work unless you are in a copy of
ArcMap document in `NPS_Transect_Map_and_Database.zip`.

5) See `Docs/NPS SOP_v2.0.5.doc` for additional details.
