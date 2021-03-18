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
  term.writeln(message);
};

window.initalise = () => {
  setTerminal();
};

