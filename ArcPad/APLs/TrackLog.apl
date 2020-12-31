<?xml version="1.0" encoding="UTF-8"?>
<ArcPad>
   <LAYER name="TrackLog.shp">
      <SYMBOLOGY>
         <SIMPLELABELRENDERER visible="false" field="SurveyID">
            <TEXTSYMBOL angle="0" fontcolor="0,0,0" font="Arial" fontsize="8" horzalignment="center" vertalignment="center" rtl="false">
            </TEXTSYMBOL>
         </SIMPLELABELRENDERER>
         <VALUEMAPRENDERER lookupfield="SegType">
            <EXACT value="OffTransect" label="Off Transect">
               <COMPLEXLINESYMBOL color="255,0,0" width="2" interval="1" offset="0" linetype="carto" pattern="MMMMMMGGGGGG">
               </COMPLEXLINESYMBOL>
            </EXACT>
            <EXACT value="OnTransect" label="On Transect">
               <COMPLEXLINESYMBOL color="56,168,0" width="3" interval="1" offset="0" linetype="carto">
               </COMPLEXLINESYMBOL>
            </EXACT>
            <OTHER label="&lt;all other values&gt;">
               <SIMPLELINESYMBOL width="2.5" color="0,148,108" type="solid">
               </SIMPLELINESYMBOL>
            </OTHER>
         </VALUEMAPRENDERER>
      </SYMBOLOGY>
   </LAYER>
</ArcPad>
