import { useEffect, useRef, useState } from 'react';

/* Scroll-reveal wrapper matching the design's `.reveal`/`.in` motion.
   delay: 0|1|2|3 -> .d1/.d2/.d3 stagger classes from the design CSS. */
export default function Reveal({ as: Tag = 'div', delay = 0, className = '', children, ...rest }) {
  const ref = useRef(null);
  const [shown, setShown] = useState(false);

  useEffect(() => {
    const el = ref.current;
    if (!el) return undefined;
    const io = new IntersectionObserver((entries) => {
      entries.forEach((e) => {
        if (e.isIntersecting) { setShown(true); io.unobserve(e.target); }
      });
    }, { threshold: 0.14 });
    io.observe(el);
    return () => io.disconnect();
  }, []);

  const d = delay ? ` d${delay}` : '';
  return (
    <Tag ref={ref} className={`reveal${d}${shown ? ' in' : ''} ${className}`.trim()} {...rest}>
      {children}
    </Tag>
  );
}
