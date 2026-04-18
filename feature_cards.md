# FairSplit Feature Cards

## Feature: Create Group

### Goal
Allow a user to create a new expense-sharing group.

### Data Ownership
**Owns:**
- Group
- Member

**Reads:**
- User

### Input
- Group name
- Currency
- Creator user id

### Output
- Created group id
- Group details
- Creator added as first group member

### Dependencies
- User must exist
- Group and creator membership should be created atomically

### Invariants
- Group name must not be empty
- Currency must be supported
- Creator automatically becomes a member of the group


## Feature: Join Group

### Goal
Allow a user to join an existing group via invite link or invite code.

### Data Ownership
**Owns:**
- Member

**Reads:**
- Group
- User
- Invite token / invite code

### Input
- Group invite token or code
- User id

### Output
- Membership confirmation
- Group details

### Dependencies
- Group must exist
- User must exist
- Invite token/code must be valid

### Invariants
- A user cannot join the same group twice
- Expired or invalid invites must be rejected
- Membership must reference a valid group and user


## Feature: Add Expense

### Goal
Allow a group member to add a shared expense and define how it is split.

### Data Ownership
**Owns:**
- Expense
- ExpenseParticipant

**Reads:**
- Group
- Member
- Balance

### Input
- Group id
- Expense description
- Amount
- Currency
- Date
- Payer member id
- Participant member ids
- Split type
- Optional custom split values or percentages

### Output
- Created expense id
- Expense details
- Participant shares
- Updated balances

### Dependencies
- Group must exist
- Payer must belong to the group
- All participants must belong to the group
- Expense, shares, and balance updates should be atomic

### Invariants
- Amount must be greater than zero
- Payer must be a valid member of the group
- Every participant must be a member of the same group
- Equal split must divide the full amount among participants
- Custom split amounts must sum exactly to total amount
- Percentage split must sum to 100
- Expense creation must not leave balances in an inconsistent state


## Feature: View Group Balances

### Goal
Allow group members to see who owes whom and the net balance per member.

### Data Ownership
**Owns:**
- Balance (if stored as a table or materialized read model)

**Reads:**
- Group
- Member
- Expense
- ExpenseParticipant
- Settlement

### Input
- Group id

### Output
- Per-member balances
- Net debt overview
- Optional settlement suggestions

### Dependencies
- Group must exist
- User must have access to the group
- Balances may be derived from expenses and settlements or stored as a read model

### Invariants
- Balance view must reflect all confirmed expenses and settlements
- Unauthorized users must not access group balances
- Reported balances must be internally consistent


## Feature: Record Settlement

### Goal
Allow one member to record that a debt between members has been settled.

### Data Ownership
**Owns:**
- Settlement

**Reads:**
- Group
- Member
- Balance

### Input
- Group id
- From member id
- To member id
- Amount
- Date
- Optional note

### Output
- Created settlement id
- Updated balances

### Dependencies
- Group must exist
- Both members must belong to the group
- Settlement and balance update should be atomic

### Invariants
- Settlement amount must be greater than zero
- Members must belong to the same group
- Settlement must not create impossible balance states
- A settlement cannot be recorded for nonexistent members


## Feature: View Expense History

### Goal
Allow group members to review all recorded expenses for transparency and auditability.

### Data Ownership
**Owns:**
- None directly (read-only feature)

**Reads:**
- Group
- Expense
- ExpenseParticipant
- Member

### Input
- Group id
- Optional filters: date range, category, payer, participant

### Output
- Ordered list of expenses
- Expense details including payer and participants

### Dependencies
- Group must exist
- User must have access to the group
- Pagination/filtering may be applied

### Invariants
- Users may only view history for groups they belong to
- Expense history ordering must be stable
- No expense should appear duplicated or be silently omitted