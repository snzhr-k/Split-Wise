export type Expense = {
	id: string;
	groupId: string;
	payerMemberId: string;
	amount: number;
	createdAtUtc: string;
};

export type CreateExpenseParticipant = {
  memberId: string;
};

export type CreateExpenseRequest = {
  payerMemberId: string;
  amount: number;
  splitType: 'equal';
  participants: CreateExpenseParticipant[];
};
