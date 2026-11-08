import Icon from '../Icon';

/* Tag multi-select matching auth.css `.tags`/`.tag`/`.on`.
   options: [{ id, name }]; value/onChange: array of selected ids. */
export default function TagMultiSelect({ options = [], value = [], onChange }) {
  const toggle = (id) => {
    onChange(value.includes(id) ? value.filter((v) => v !== id) : [...value, id]);
  };
  return (
    <div className="tags">
      {options.map((o) => {
        const on = value.includes(o.id);
        return (
          <button key={o.id} type="button" className={`tag${on ? ' on' : ''}`} onClick={() => toggle(o.id)} aria-pressed={on}>
            {o.name}
            <Icon name="x" size={13} stroke={2.4} className="tg-x" />
          </button>
        );
      })}
    </div>
  );
}
