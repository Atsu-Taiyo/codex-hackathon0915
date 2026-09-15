export const words = ['robot', 'box', 'push', 'pull', 'lift', 'open', 'shake', 'break'];
export function validateInput(value) {
  if (!value || !Array.isArray(value.cards) || value.cards.length !== 3 ||
      !value.cards.every(w => w === null || w === '' || words.includes(w)) ||
      !Array.isArray(value.vocabulary) || !value.vocabulary.every(w => words.includes(w)))
    throw new Error('Invalid card state');
  if (value.text !== undefined && (typeof value.text !== 'string' || !value.text.trim() || value.text.length > 2000))
    throw new Error('Invalid transcript');
  return value;
}
export function validateOrder(result, vocabulary) {
  if (!Array.isArray(result.cards) || result.cards.length !== 3 ||
      !result.cards.every(w => w === '' || vocabulary.includes(w)) ||
      new Set(result.cards.filter(Boolean)).size !== result.cards.filter(Boolean).length)
    throw new Error('Codex returned invalid or undiscovered cards');
  return result.cards;
}
