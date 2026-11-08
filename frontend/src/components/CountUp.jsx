import { useEffect, useRef, useState } from 'react';

/* Animated count-up matching the design (1.2s ease-out, en-US grouping).
   Starts when scrolled into view. */
export default function CountUp({ to = 0, suffix = '', decimals = 0, duration = 1200, className }) {
  const [val, setVal] = useState(0);
  const ref = useRef(null);
  const started = useRef(false);

  useEffect(() => {
    const el = ref.current;
    if (!el) return undefined;
    const io = new IntersectionObserver((entries) => {
      entries.forEach((e) => {
        if (e.isIntersecting && !started.current) {
          started.current = true;
          const start = performance.now();
          const tick = (now) => {
            const p = Math.min((now - start) / duration, 1);
            const eased = 1 - Math.pow(1 - p, 3);
            setVal(to * eased);
            if (p < 1) requestAnimationFrame(tick);
            else setVal(to);
          };
          requestAnimationFrame(tick);
        }
      });
    }, { threshold: 0.3 });
    io.observe(el);
    return () => io.disconnect();
  }, [to, duration]);

  const display = decimals > 0
    ? val.toFixed(decimals)
    : Math.round(val).toLocaleString('en-US');

  return <span ref={ref} className={className}>{display}{val >= to ? suffix : ''}</span>;
}
