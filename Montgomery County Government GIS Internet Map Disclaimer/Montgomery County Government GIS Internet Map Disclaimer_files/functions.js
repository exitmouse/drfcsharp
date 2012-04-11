<!-- Hide Script...
function VersionBrowser(Netscape, Explorer)
{
        if ((navigator.appVersion.substring(0,3) >= Netscape && navigator.appName == 'Netscape') ||
            (navigator.appVersion.substring(0,3) >= Explorer && navigator.appName.substring(0,9) == 'Microsoft'
))
        {
                return true;
        }
        else
        {
                return false;
        }
}

function countLetters()
{
        var query = document.forms[0].query.value;

        if (query.length<2)
        {
                // Likely to be no words at all
                return(false);
        }
        else
        {
                return(true);
        }
}

function checkForm()
{
        x=countWords();
        y=countLetters();

        if (VersionBrowser(3.0,4.0))
        {
                if (y==true)
                {
                        if(x==true)
                        {
                                return(true);
                        }
                        else if (x==false)
                        {
                                msg = "For better results it is recommended that\n"+
                                "you describe your interest using a sentence.\n"+
                                "Click on OK to continue,\nor click on cancel to enter more text.\n";
                                z=confirm(msg);
                                return z;
                        }
                }
                else
                {
                        msg = "You must enter some query text!\n";
                        alert(msg);
                        return(false);
                }
        }
        else
        {
                return(true);
        }
}

 function gotoURL() { 
     var Current = document.ProgramForm.Program.selectedIndex; 
     location.href = 
document.ProgramForm.Program.options[Current].value; 
     return false; 
 } 


function countWords()
{
        var query = document.forms[0].query.value;

        var found=0;
        var i;

        for (i=0; i<query.length;i++)
        {
                x=query.charAt(i);
                if (x==" ")
                {
                        found++;
                }
        }
        if (found<1)
        {
                return false;
        }
        else
        {
                return true;
        }
}

function openScript(url, width, height) {
	var win = window.open("http://www.montgomerycountymd.gov/"+ url, "openScript", 'width=' + width + ',height=' + height +',scrollbars=yes,toolbar=yes,status=yes,menubar=yes,location=yes,resizable=yes');
}

function openScriptPopUp(url, width, height) {
	var win = window.open(url, "openScript", 'width=' + width + ',height=' + height +',scrollbars=yes,toolbar=yes,status=yes,menubar=yes,location=yes,resizable=yes');
}


function MM_swapImgRestore() { //v3.0
  var i,x,a=document.MM_sr; for(i=0;a&&i<a.length&&(x=a[i])&&x.oSrc;i++) x.src=x.oSrc;
}

function MM_preloadImages() { //v3.0
  var d=document; if(d.images){ if(!d.MM_p) d.MM_p=new Array();
    var i,j=d.MM_p.length,a=MM_preloadImages.arguments; for(i=0; i<a.length; i++)
    if (a[i].indexOf("#")!=0){ d.MM_p[j]=new Image; d.MM_p[j++].src=a[i];}}
}

function MM_findObj(n, d) { //v4.0
  var p,i,x;  if(!d) d=document; if((p=n.indexOf("?"))>0&&parent.frames.length) {
    d=parent.frames[n.substring(p+1)].document; n=n.substring(0,p);}
  if(!(x=d[n])&&d.all) x=d.all[n]; for (i=0;!x&&i<d.forms.length;i++) x=d.forms[i][n];
  for(i=0;!x&&d.layers&&i<d.layers.length;i++) x=MM_findObj(n,d.layers[i].document);
  if(!x && document.getElementById) x=document.getElementById(n); return x;
}

function MM_swapImage() { //v3.0
  var i,j=0,x,a=MM_swapImage.arguments; document.MM_sr=new Array; for(i=0;i<(a.length-2);i+=3)
   if ((x=MM_findObj(a[i]))!=null){document.MM_sr[j++]=x; if(!x.oSrc) x.oSrc=x.src; x.src=a[i+2];}
}

function MM_jumpMenu(targ,selObj,restore){ //v3.0
  eval(targ+".location='"+selObj.options[selObj.selectedIndex].value+"'");
  if (restore) selObj.selectedIndex=0;
}
function createWindow(cUrl,cName,cFeatures) {
//var xWin = window.open("http://www.montgomerycountymd.gov"+ cUrl,cName,cFeatures)
var xWin = window.open(""+ cUrl,cName,cFeatures)
}


function updateLinks(){ 
            for (var i=0; i < document.links.length; i++){ 
                        //alert("BEFORE " + document.links[i]);
                        var linkStr = new String(document.links[i]) ; 
                        linkStr = linkStr.replace('%23', '#')
						linkStr = linkStr.replace('%25', '%')
                        document.links[i].href  = linkStr
                        //alert("AFTER " + document.links[i]);
            } 
}

function GotoSearch(theForm, theField){
	with(theForm){
	searchVal=theField.value
	url_str="http://207.96.37.108/search?q=" + escape(searchVal) + "&restrict=&btnG=Google+Search&ie=&site=my_collection&output=xml_no_dtd&client=my_collection&lr=&proxystylesheet=my_collection&oe=";
	window.open(url_str, null, "right=0,top=0,left=0,toolbar=yes,location=No,scrollbars=yes,status=yes,resizable=yes,fullscreen=No", false);
	}
}

//finished Script hiding-->
