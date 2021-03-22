var term;

window.setTerminal = () => {
  term = new Terminal({cursorBlink: true});
  term.open(document.getElementById('terminal'));
  fitAddon = new FitAddon.FitAddon();
  term.loadAddon(fitAddon);
  fitAddon.fit();
  term.writeln('Vatsim UK Sector File Compiler..');
};

window.writeLine = (message) => {
  var msg = message.toLowerCase();
  if (msg.includes("info")) {
    term.writeln('\x1b[34;1m' + message);
  } else if (msg.includes("success")) {
    term.writeln('\x1b[32;1m' + message);
  } else if (msg.includes("error")) {
    term.writeln('\x1b[31;1m' + message);
  } else {
    term.writeln(message);
  }
  
};

window.initalise = () => {
  setTerminal();
};

