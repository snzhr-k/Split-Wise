export function formatCurrency(amount: number) {
	return new Intl.NumberFormat(undefined, {
		style: 'currency',
		currency: 'USD',
		maximumFractionDigits: 2,
	}).format(amount);
}
