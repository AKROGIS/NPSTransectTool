<?xml version="1.0" encoding="UTF-8"?>
<ArcPad>
   <LAYER name="TrnOrig.shp">
      <SYMBOLOGY>
         <SIMPLELABELRENDERER visible="false" field="PROJECTION">
            <TEXTSYMBOL angle="0" fontcolor="0,0,0" font="Arial" fontsize="8" horzalignment="center" vertalignment="center" rtl="false">
            </TEXTSYMBOL>
         </SIMPLELABELRENDERER>
         <VALUEMAPRENDERER lookupfield="Flown">
            <EXACT value="N" label="N">
               <COMPLEXLINESYMBOL color="0,92,230" width="1.5" interval="1" offset="0" linetype="carto">
               </COMPLEXLINESYMBOL>
            </EXACT>
            <EXACT value="P" label="P">
               <GROUPSYMBOL>
                  <COMPLEXLINESYMBOL color="0,0,0" width="0.4" interval="1" offset="0" linetype="carto" pattern="MMMMMMGMMGMMG">
                  </COMPLEXLINESYMBOL>
                  <COMPLEXLINESYMBOL color="0,255,197" width="4" interval="1" offset="0" linetype="carto">
                  </COMPLEXLINESYMBOL>
               </GROUPSYMBOL>
            </EXACT>
            <EXACT value="Y" label="Y">
               <GROUPSYMBOL>
                  <COMPLEXLINESYMBOL color="204,204,204" width="2.6" interval="1" offset="0" linetype="carto" pattern="MMMMMGGGGG">
                  </COMPLEXLINESYMBOL>
                  <COMPLEXLINESYMBOL color="0,0,0" width="3.4" interval="1" offset="0" linetype="carto">
                  </COMPLEXLINESYMBOL>
               </GROUPSYMBOL>
            </EXACT>
            <OTHER label="&lt;all other values&gt;">
               <SIMPLELINESYMBOL width="1" color="0,148,108" type="solid">
               </SIMPLELINESYMBOL>
            </OTHER>
         </VALUEMAPRENDERER>
      </SYMBOLOGY>
   </LAYER>
</ArcPad>
