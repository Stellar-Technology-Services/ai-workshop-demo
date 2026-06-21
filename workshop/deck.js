// Minimal slideshow engine. No dependencies; works from file://.
// Keys: → / Space / PageDown = next · ← / PageUp = prev · Home/End = first/last
//       f = fullscreen · n = toggle presenter notes
(function () {
  const deck = document.querySelector(".deck");
  const slides = Array.from(document.querySelectorAll(".slide"));
  if (!deck || slides.length === 0) return;

  // Progress bar + slide counter chrome.
  const track = document.createElement("div");
  track.className = "progress-track";
  const bar = document.createElement("div");
  bar.className = "progress";
  track.appendChild(bar);
  document.body.appendChild(track);

  const chrome = document.createElement("div");
  chrome.className = "chrome";
  const left = document.createElement("span");
  const right = document.createElement("span");
  chrome.append(left, right);
  document.body.appendChild(chrome);
  left.textContent = document.title;

  // Restore position within a session via the URL hash (survives reload).
  let i = clamp(parseInt(location.hash.replace("#", ""), 10) - 1 || 0);

  function clamp(n) { return Math.max(0, Math.min(slides.length - 1, n)); }

  function show(n) {
    i = clamp(n);
    slides.forEach((s, idx) => s.classList.toggle("active", idx === i));
    right.textContent = `${i + 1} / ${slides.length}`;
    bar.style.width = `${((i + 1) / slides.length) * 100}%`;
    history.replaceState(null, "", `#${i + 1}`);
  }

  function next() { show(i + 1); }
  function prev() { show(i - 1); }

  document.addEventListener("keydown", (e) => {
    switch (e.key) {
      case "ArrowRight":
      case "PageDown":
      case " ":
        e.preventDefault(); next(); break;
      case "ArrowLeft":
      case "PageUp":
        e.preventDefault(); prev(); break;
      case "Home": e.preventDefault(); show(0); break;
      case "End": e.preventDefault(); show(slides.length - 1); break;
      case "n": case "N": deck.classList.toggle("show-notes"); break;
      case "f": case "F":
        if (!document.fullscreenElement) document.documentElement.requestFullscreen();
        else document.exitFullscreen();
        break;
    }
  });

  // Click/tap the right 2/3 to advance, left 1/3 to go back.
  document.addEventListener("click", (e) => {
    if (e.target.closest("a")) return;
    if (e.clientX < window.innerWidth / 3) prev(); else next();
  });

  show(i);
})();
