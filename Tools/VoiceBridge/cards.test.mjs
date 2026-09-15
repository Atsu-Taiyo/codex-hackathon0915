import { test } from 'node:test';
import assert from 'node:assert/strict';
import { validateInput, validateOrder } from './cards.mjs';
const vocabulary = ['robot', 'box', 'lift'];
test('accepts an available arrangement and empty slots', () => {
  assert.deepEqual(validateOrder({ cards: ['box', 'lift', 'robot'] }, vocabulary), ['box', 'lift', 'robot']);
  assert.deepEqual(validateOrder({ cards: ['', 'lift', ''] }, vocabulary), ['', 'lift', '']);
});
test('rejects duplicate, unknown, undiscovered and malformed cards', () => {
  for (const cards of [['robot', 'lift', 'robot'], ['robot', 'open', 'box'], ['robot'], [null, 'lift', 'box']])
    assert.throws(() => validateOrder({ cards }, vocabulary));
});
test('rejects malformed inputs and oversized speech', () => {
  const input = { cards: [null, '', 'lift'], vocabulary };
  assert.equal(validateInput(input), input);
  assert.throws(() => validateInput({ ...input, text: 'x'.repeat(2001) }));
  assert.throws(() => validateInput({ ...input, cards: ['malicious', '', ''] }));
});
