# BlerghMerge
BlerghMerge automatically merges HTML, CSS and JS into a single file using a simple header system and a single run command.
The file will work the same before and after merging, but will have a faster initial load after merging.

## Usage
BlerghMerge is able to merge multiple HTML, CSS and JavaScript files.

### Calling the tool
The .exe takes two arguments, where one is optional.  
```BlerghMerge.exe [SOURCE DIRECTORY] [OPTIONAL TARGET DIRECTORY]```  
If no target directory is specified, it's placed next to the source with the name ```blergh output```


### CSS
All BLERGH is called upon using the `BLERGH` or `BLERGH!` header  
```
<!-- BLERGH -->
<link rel="stylesheet" href="../css/main.css" type="text/css"/>
```
This will replace the link to the style sheet with the actual style sheet inside ```style``` tags

### JavaScript
JavaScript works in a very similar way.
```
<!-- BLERGH -->
<link rel="stylesheet" href="../css/main.css" type="text/css"/>
```
This will put the linked JavaScript inside ```script``` tags.


### HTML
HTML is a bit more complex, and it's identified by the class ```imported```
<!-- BLERGH -->
<div class="imported">..\header.html</div>
```
BlerghMerge will replace the entire element with the linked HTML. Because HTML can not be called directly it's recommended to put this into your JavaScript.
```
// BLERGH IGNORE
var importedItems = document.getElementsByClassName("imported");
for (var i=0;i<importedItems.length;i++) {
    var item = importedItems[i];
    var ajax = new XMLHttpRequest();
    ajax.open("GET", item.innerHTML, false);
    ajax.send();
    item.outerHTML = ajax.responseText;
}
// BLERGH IGNORE END
```
This will load your HTML into the un-merged document when working on it.
