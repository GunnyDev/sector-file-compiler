(()=>{var e;window.setTerminal=()=>{(e=new Terminal({cursorBlink:!0})).open(document.getElementById("terminal")),fitAddon=new FitAddon.FitAddon,e.loadAddon(fitAddon),fitAddon.fit(),e.writeln("Vatsim UK Sector File Compiler..")},window.writeLine=i=>{var n=i.toLowerCase();n.includes("info")?e.writeln("[34;1m"+i):n.includes("success")?e.writeln("[32;1m"+i):n.includes("error")?e.writeln("[31;1m"+i):e.writeln(i)},window.initalise=()=>{setTerminal()}})();