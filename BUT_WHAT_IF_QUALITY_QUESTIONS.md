# SplitWise: "But what if...?" Questions Mapped to Quality Attributes

Date: 2026-03-14
Scope: Android/iOS SplitWise application

This document reframes user stories as risk and design questions. Each question is linked to the quality attributes that should guide architecture, implementation, and testing.

## Quality Attribute Legend
- Performance
- Scalability
- Availability
- Security
- Modifiability
- Testability
- Usability
- Deployability

## Group Management (US-01 to US-06)

| User Story | But what if...? | Important Quality Attributes | Why these attributes matter |
|---|---|---|---|
| US-01 Create group | But what if two users try to create groups with the same invite code pattern at the same time? | Security, Scalability, Testability | Invite code uniqueness and collision handling must be safe and verifiable under concurrent load. |
| US-01 Create group | But what if group creation is attempted while the user is offline or has unstable mobile data? | Availability, Usability, Testability | Users need clear behavior (retry/queue/fail) and predictable UX for poor connectivity. |
| US-02 Join via invite | But what if an invite link is leaked publicly and many unknown users attempt to join? | Security, Scalability, Availability | Access control, rate limiting, and service resilience are required to prevent abuse and outages. |
| US-02 Join via invite | But what if a QR/invite expires while the user is in the join flow? | Usability, Security | The app should fail safely and guide the user with clear recovery steps. |
| US-03 Leave group | But what if a member leaves while they still owe money or are owed money? | Usability, Modifiability, Testability | Business rules for exit constraints must be clear and maintainable as policy evolves. |
| US-04 Edit group settings | But what if two admins update currency/name at nearly the same time? | Availability, Testability, Usability | Conflict handling and consistent state are needed to avoid confusion and data drift. |
| US-05 Delete group | But what if an admin accidentally deletes an active group with unresolved balances? | Usability, Security, Testability | Destructive actions need confirmation, authorization checks, and recovery behavior. |
| US-06 Remove member | But what if removing a member would invalidate historical expenses they participated in? | Modifiability, Testability, Usability | Data model and rules should preserve history while enforcing membership constraints. |

## Expense Management (US-07 to US-13)

| User Story | But what if...? | Important Quality Attributes | Why these attributes matter |
|---|---|---|---|
| US-07 Add expense | But what if the same expense is submitted twice due to double-tap or retry after timeout? | Testability, Usability, Availability | Idempotent create behavior and clear UX prevent duplicate records. |
| US-07 Add expense | But what if thousands of expenses exist and adding a new one slows down dramatically? | Performance, Scalability | Write path and indexing must scale as group history grows. |
| US-08 Specify payer | But what if payer selected is no longer an active member due to stale local cache? | Availability, Testability, Usability | Sync and validation logic should prevent invalid entries and explain errors clearly. |
| US-09 Choose participants | But what if no participant is selected, or payer is excluded unintentionally? | Usability, Testability | Input validation and helpful defaults prevent incorrect debt calculations. |
| US-10 Split equally/custom/percent | But what if custom or percentage splits do not sum to total because of rounding? | Testability, Modifiability, Usability | Deterministic rounding and validation rules must be easy to maintain and explain. |
| US-10 Split equally/custom/percent | But what if split mode changes after participants are edited mid-form? | Usability, Testability | State transitions need to remain predictable to avoid silent data corruption. |
| US-11 Edit expense | But what if editing a historical expense changes many balances and triggers conflicts with concurrent edits? | Availability, Performance, Testability | Recalculation consistency and conflict control are key under real-time collaboration. |
| US-12 Delete expense | But what if deleting an expense that has related settlements causes negative or inconsistent balances? | Testability, Modifiability, Usability | Referential and domain integrity rules must be enforced and understandable. |
| US-13 Metadata fields | But what if users enter very long descriptions, unusual categories, or timezone-crossing dates? | Performance, Usability, Testability | Input constraints, date normalization, and rendering behavior should be robust. |

## Balances and Settlement (US-14 to US-17)

| User Story | But what if...? | Important Quality Attributes | Why these attributes matter |
|---|---|---|---|
| US-14 Per-user balance | But what if balance shown on one device differs from another due to sync lag? | Availability, Usability, Testability | Consistency model and freshness indicators are needed for user trust. |
| US-15 Group debt overview | But what if debt graph computation becomes slow for large groups and long history? | Performance, Scalability | Aggregation strategy must support fast reads as data volume increases. |
| US-16 Record settlement | But what if a settlement is recorded twice by both parties at nearly the same time? | Testability, Availability, Usability | Duplicate detection and concurrency handling avoid incorrect balance updates. |
| US-16 Record settlement | But what if settlement amount exceeds outstanding debt due to stale data? | Usability, Testability, Security | Validation should prevent overpayment manipulation and accidental errors. |
| US-17 Minimal transactions | But what if optimization suggests mathematically minimal transfers that are socially confusing? | Usability, Modifiability | Algorithm output may need explainability and policy tuning over time. |
| US-17 Minimal transactions | But what if optimization time grows too much for large groups? | Performance, Scalability, Deployability | Efficient algorithms and deployable computation strategy (client/server) are required. |

## Insights and Transparency (US-18 to US-20)

| User Story | But what if...? | Important Quality Attributes | Why these attributes matter |
|---|---|---|---|
| US-18 Expense history | But what if history pagination fails and users see missing or duplicated records? | Availability, Testability, Usability | Reliable pagination and ordering are essential for auditability. |
| US-19 Analytics by category/member/time | But what if analytics queries are too slow on mobile networks? | Performance, Scalability, Usability | Response time and payload size directly affect mobile experience. |
| US-19 Analytics by category/member/time | But what if category definitions change after release (merge, rename, custom categories)? | Modifiability, Deployability, Testability | Schema and migration design should evolve safely without breaking dashboards. |
| US-20 Personal contribution view | But what if users misinterpret "you paid" vs "you owe" due to unclear visual language? | Usability, Testability | High-stakes financial UI requires clear semantics and user-validated design. |
| US-18/19/20 Reporting access | But what if a removed member can still access cached insights for a private group? | Security, Availability, Testability | Authorization, cache invalidation, and session handling must protect sensitive data. |

## Cross-Cutting Mobile/Platform Questions

| Area | But what if...? | Important Quality Attributes | Why these attributes matter |
|---|---|---|---|
| Offline mode | But what if users add expenses offline for hours and conflicts appear when reconnecting? | Availability, Testability, Usability | Conflict resolution policy and user messaging determine trust and correctness. |
| Notifications | But what if push notifications are delayed, duplicated, or out of order? | Availability, Usability, Testability | Event consistency and client handling should avoid user confusion. |
| Authentication | But what if a stolen device has an active session into private groups? | Security, Usability | Session expiry, re-auth, and safe lock behavior are critical for financial privacy. |
| Release process | But what if Android and iOS versions diverge in split logic after staggered release? | Deployability, Testability, Modifiability | Version compatibility and contract tests prevent cross-platform balance mismatches. |
| Observability | But what if balance mismatches happen but logs/metrics cannot explain why? | Testability, Availability, Modifiability | Traceability and diagnostics are needed for rapid incident resolution. |

## Suggested Prioritization (First NFR Candidates)

1. **Security + Availability for invite/join and access control** (US-02, reporting access).
2. **Correctness and testability for split and balance math** (US-10, US-14, US-16, US-17).
3. **Performance/scalability for history, analytics, and optimization** (US-15, US-19, US-17).
4. **Usability for high-risk actions and financial comprehension** (US-05, US-20).
5. **Deployability/modifiability for evolving categories and cross-platform parity** (US-19, mobile release process).

## How to Use This in Your Project

- Convert each row into a non-functional requirement or acceptance criterion.
- Add at least one validation method per row: automated test, load test, security test, UX study, or release gate.
- Track coverage per quality attribute to avoid over-focusing only on functional stories.

## Quality Attribute Scenarios

Each row below is a concrete quality attribute scenario for one previously defined "But what if...?" question.

| Scenario ID | User Story ID | Quality Attribute | Source | Stimulus | Artifact | Environment | Response | Response Measure |
|---|---|---|---|---|---|---|---|---|
| QAS-01 | US-01 | Security | Concurrent group creators | Two create-group requests generate the same invite code candidate | Invite code generation and group creation API | Peak traffic, 100 parallel create requests | System atomically enforces unique invite codes and retries code generation | 0 duplicate invite codes in 1,000,000 creations; API conflict handling < 300 ms p95 |
| QAS-02 | US-01 | Availability | Mobile client with weak network | User submits create-group while offline/intermittent | Mobile create-group flow and sync queue | Airplane mode toggled during submit | App queues request or fails clearly and allows retry without data loss | 100% of interrupted submissions recover via retry; no orphan groups created |
| QAS-03 | US-02 | Security | External attacker/bot traffic | Leaked invite link receives high-volume join attempts | Invite validation endpoint and membership service | 2,000 join attempts/min from mixed IPs | System rate-limits abuse, blocks invalid attempts, and keeps service responsive for valid users | >= 99% malicious attempts blocked; valid join success >= 99%; p95 latency < 500 ms |
| QAS-04 | US-02 | Usability | Legitimate user opening old invite | Invite token is expired during join flow | Invite join screen and backend token validator | Normal usage after token TTL elapsed | App shows explicit expiration reason and one-tap recovery action (request new invite) | 100% expired tokens show actionable error; task completion with new invite <= 60 s median |
| QAS-05 | US-03 | Usability | Group member with unsettled balance | Member attempts to leave group with non-zero net balance | Leave-group business rules and UI | Active group with open debts/credits | System blocks leaving, explains why, and offers settlement navigation | 0 invalid leaves with open balance; >= 90% users understand next action in usability test |
| QAS-06 | US-04 | Availability | Two admins | Admin A and B update currency/name simultaneously | Group settings write path and conflict resolver | Concurrent writes within 2 seconds | System detects conflict and applies deterministic policy (version check/last-write with warning) | No partial updates; 100% conflicts produce visible resolution message |
| QAS-07 | US-05 | Security | Group admin performing destructive action | Admin taps delete on active group | Group deletion endpoint and authorization policy | Authenticated session, unresolved balances present | System requires elevated confirmation and enforces role + policy checks before delete | 0 unauthorized deletions; accidental-delete recovery window >= 24 h if soft-delete enabled |
| QAS-08 | US-06 | Modifiability | Product policy change | Rule changes from hard-remove to archive-member behavior | Membership model and historical expense references | Existing production data with old member links | System supports new rule without rewriting historical expenses | Migration completes with 0 history loss; change implemented in <= 2 sprints |
| QAS-09 | US-07 | Availability | End user double tap/retry | Same add-expense request is submitted twice | Expense creation API and idempotency key handling | Mobile timeout then immediate retry | System stores only one expense and returns same logical result to retries | Duplicate expense rate < 0.01%; idempotent retry success >= 99.9% |
| QAS-10 | US-07 | Performance | Large active group | User submits new expense in group with 100k expense records | Expense write path, indexes, and event pipeline | Warm database, concurrent reads/writes | System persists expense and updates derived views within target latency | Add-expense API p95 < 400 ms at 100 req/s |
| QAS-11 | US-08 | Availability | Mobile app with stale membership cache | User chooses payer who was recently removed | Payer validation and member sync logic | Cache is stale by up to 10 minutes | System rejects invalid payer, refreshes member list, preserves form data | 100% invalid payers rejected; form data retention >= 99% after refresh |
| QAS-12 | US-09 | Usability | End user data entry | User submits with empty participant set or missing payer-participant consistency | Expense input validation layer | Standard add-expense flow | System blocks submit and highlights exact corrective fields | 100% invalid forms prevented; correction completion <= 20 s median |
| QAS-13 | US-10 | Testability | QA automation suite | Percentage/custom split totals differ from amount due to rounding | Split calculation engine | Multi-currency and decimal edge cases | System applies deterministic rounding strategy and exposes explainable per-user amounts | 100% deterministic results across platforms; 0 flaky math tests in CI |
| QAS-14 | US-10 | Usability | End user editing draft | User changes split mode after editing participants and amounts | Expense form state machine | Mid-form mode transitions | System preserves valid inputs when possible and warns before destructive recalculation | 0 silent value drops; >= 95% task success in moderated usability run |
| QAS-15 | US-11 | Availability | Two members editing same expense | Concurrent edits arrive with outdated version from one client | Expense update API and optimistic locking | Real-time collaboration in active group | System rejects stale write with conflict response and offers merge/reload path | 100% stale updates detected; no lost update incidents in concurrency test |
| QAS-16 | US-12 | Testability | Member deleting expense | Expense tied to downstream settlements is deleted | Domain integrity constraints and balance recomputation | Production-like dataset with linked records | System enforces referential rules and prevents inconsistent net balances | 0 integrity violations in deletion test suite; balance invariants always hold |
| QAS-17 | US-13 | Performance | User enters extreme metadata | Very long description, custom category, and cross-timezone date submitted | Input parser, storage, and date normalization | Mixed locale/timezone devices | System validates length, normalizes dates to canonical timezone, and stores efficiently | Validation response p95 < 200 ms; date display consistency 100% across timezones |
| QAS-18 | US-14 | Availability | Multiple user devices | Device A and B show different balances after recent updates | Balance sync and read model freshness | Eventual consistency with intermittent connectivity | System marks stale data, triggers sync, and converges to same value | Cross-device balance divergence resolves within 5 s p95 |
| QAS-19 | US-15 | Performance | Large group view request | User opens "who owes whom" for 500 members, long history | Debt graph computation and query layer | Heavy data volume, normal mobile network | System returns summarized debt graph quickly using pre-aggregations | Debt overview load p95 < 1.5 s |
| QAS-20 | US-16 | Testability | Two parties recording same payment | Both members record equivalent settlement near-simultaneously | Settlement deduplication and ledger write path | Concurrent submit within 3 seconds | System detects probable duplicate and merges/flags before applying twice | Duplicate settlement defects = 0 in concurrency tests |
| QAS-21 | US-16 | Security | Malicious or mistaken user input | Settlement amount exceeds outstanding debt | Settlement validator and authorization rules | Stale client balance and manual amount edit | System rejects over-settlement and logs security-relevant event | 100% over-limit attempts blocked; audit log written for 100% rejects |
| QAS-22 | US-17 | Usability | Group member interpreting recommendations | Minimal-transfer output is mathematically correct but hard to understand | Settlement suggestion UI and explanation text | Group with circular debts | System provides explanation and optional "simpler, not minimal" mode | >= 85% users correctly explain recommendation in UX validation |
| QAS-23 | US-17 | Performance | Large optimization request | Compute minimal transactions for 1,000 participants | Optimization engine | Server under moderate concurrent load | System computes within bounded time or degrades to heuristic mode | Exact/heuristic response returned <= 2 s p95 |
| QAS-24 | US-18 | Availability | User scrolling history | Pagination token/order mismatch causes missing or duplicate rows | Expense history API and client paginator | High write churn during read | System provides stable ordering and deduplicated pagination | 0 missing/duplicate rows in pagination consistency tests |
| QAS-25 | US-19 | Performance | Mobile user on slow network | User requests analytics with broad date range and multiple filters | Analytics query service and payload formatter | 3G-equivalent network | System returns incremental or cached result with compact payload | First analytics render <= 2.5 s p95 on 3G profile |
| QAS-26 | US-19 | Modifiability | Product owner changes taxonomy | Category rename/merge/customization must apply without breaking reports | Category model, migration scripts, analytics aggregations | App versions N and N-1 both active | System supports backward-compatible category mapping and migration | 0 broken dashboards after migration; rollback possible in < 10 min |
| QAS-27 | US-20 | Usability | Member reviewing personal summary | UI labels/colors cause confusion between "paid" and "owes" | Personal contribution screen | Normal day-to-day usage | System uses unambiguous language, signs, and visual encoding with legend/tooltips | Misinterpretation rate < 5% in usability test |
| QAS-28 | US-18, US-19, US-20 | Security | Removed member with cached data | Removed member reopens app and requests old report endpoints | Report APIs, cache invalidation, local storage | Membership revoked moments earlier | System revokes access token claims, invalidates cache, and blocks protected reads | 100% revoked users denied within 60 s; no private data returned |
| QAS-29 | US-07, US-11, US-14, US-16 | Availability | Offline mobile client | User performs multiple writes offline, reconnects after hours | Offline queue, sync conflict resolver, ledger recomputation | Long offline window, concurrent remote updates | System syncs changes with deterministic conflict policy and user-visible resolution | Offline sync success >= 99%; unresolved conflicts < 1% |
| QAS-30 | US-14, US-15, US-18 | Availability | Push infrastructure and app lifecycle | Notifications arrive delayed/duplicated/out of order | Notification handler and refresh triggers | App background/foreground transitions | System deduplicates events and fetches authoritative state before showing totals | 0 user-visible double-application of same event |
| QAS-31 | US-02, US-18 | Security | Lost or stolen device | Unauthorized person opens app with active session | Auth session manager and sensitive screens | Device unlocked, app token not yet expired | System requires re-auth for sensitive actions and supports remote session revoke | 100% sensitive actions gated by re-auth after risk trigger |
| QAS-32 | US-10, US-14, US-17 | Deployability | Staggered mobile rollout | Android and iOS clients run different app versions with logic changes | API contracts, calculation engine compatibility layer | 20% phased release on one platform | System preserves result parity via versioned contracts and server-side canonical math | Cross-platform balance variance = 0 in canary monitoring |
| QAS-33 | US-14, US-15, US-16 | Testability | Support/operations team | Reported balance mismatch cannot be reproduced quickly | Observability stack (logs, traces, audit events) | Production incident conditions | System emits correlation IDs and step-level ledger events for each mutation | Root cause identified within 30 min for >= 90% incidents |

## Scenario Prioritization (Business Importance vs Technical Risk)

Scoring model:
- Business importance: High / Medium / Low
- Technical risk: High / Medium / Low
- Architectural driver: scenarios rated High + High

| Scenario | Given Scenario | Business Importance | Technical Risk | Architectural Driver | Why? |
|---|---|---|---|---|---|
| QAS-01 | Unique invite code under concurrent group creation | Medium | Medium | No | Important for join reliability, but implementation is usually straightforward with DB uniqueness + retry. |
| QAS-02 | Create group under offline/intermittent mobile network | High | Medium | No | Core mobile expectation; failure hurts onboarding, but queue/retry patterns are well-known. |
| QAS-03 | Leaked invite link receives bot-scale join abuse | High | High | Yes | Directly impacts trust, abuse resistance, and uptime; mis-design can cause security and availability incidents. |
| QAS-04 | Expired invite during join flow | Medium | Low | No | Mostly UX clarity and token validation behavior; low architectural uncertainty. |
| QAS-05 | Member leaves group with unresolved balances | High | Medium | No | Financial correctness and user trust are critical; policy is clear but needs consistent enforcement. |
| QAS-06 | Concurrent admin edits on group settings | Medium | Medium | No | Concurrency correctness matters, but conflict handling patterns are established. |
| QAS-07 | Accidental/unauthorized delete of active group | High | Medium | No | High business impact due to destructive data loss risk; controls are standard but must be done carefully. |
| QAS-08 | Change from remove-member to archive-member policy | Medium | Medium | No | Product evolution is expected; data compatibility risk exists but typically manageable with migrations. |
| QAS-09 | Duplicate add-expense due to retries/double-tap | High | Medium | No | Duplicate ledger entries directly break trust; idempotency is necessary but technically common. |
| QAS-10 | Add-expense latency in very large histories | High | High | Yes | Expense entry is a primary user action; performance collapse at scale is a core architectural risk. |
| QAS-11 | Stale cache allows invalid payer selection | Medium | Medium | No | Frequent mobile sync edge case; impactful but controllable with validation + refresh strategy. |
| QAS-12 | Invalid participant/payer combinations in form | Medium | Low | No | Mostly input validation and UX affordances, with low technical complexity. |
| QAS-13 | Rounding determinism across split calculations | High | High | Yes | Financial math correctness across Android/iOS/backend is foundational; subtle errors are costly and hard to unwind. |
| QAS-14 | State integrity when split mode changes mid-form | Medium | Medium | No | Important UX/data consistency concern, but bounded to client state management. |
| QAS-15 | Lost updates from concurrent expense edits | High | High | Yes | Multi-user correctness issue with direct financial impact; requires robust versioning/conflict strategy. |
| QAS-16 | Delete expense with downstream settlement dependencies | High | High | Yes | Referential/ledger integrity is a core domain invariant; mistakes cause persistent balance corruption. |
| QAS-17 | Extreme metadata input and timezone normalization | Medium | Medium | No | Robustness issue with moderate impact; technical solutions are known. |
| QAS-18 | Cross-device balance divergence from sync lag | High | High | Yes | Visible inconsistency undermines trust quickly; requires carefully designed consistency model and sync behavior. |
| QAS-19 | Debt overview performance for large groups | High | High | Yes | Balance overview is a key decision surface; slow graph computation drives abandonment in big groups. |
| QAS-20 | Duplicate settlement from near-simultaneous entry | High | High | Yes | Settlement integrity is critical and concurrency defects are difficult to detect post-facto. |
| QAS-21 | Over-settlement attempts (malicious or accidental) | High | Medium | No | Strong business impact due to financial manipulation risk; validation design is clear but must be strict. |
| QAS-22 | Minimal-transfer suggestions are hard to understand | Medium | Medium | No | Impacts adoption of settlement recommendations; primarily UX and product-policy tuning. |
| QAS-23 | Optimization runtime for very large participant sets | Medium | High | No | Algorithmic complexity risk is high, but this path is less frequent than core expense flows. |
| QAS-24 | History pagination returns missing/duplicate rows | High | Medium | No | Auditability and trust rely on correct history; implementation risk is moderate with stable cursor design. |
| QAS-25 | Slow analytics over weak mobile networks | Medium | Medium | No | Valuable for insights, but not as critical as ledger correctness; cache/incremental strategies reduce risk. |
| QAS-26 | Category taxonomy evolution without report breakage | Medium | Medium | No | Long-term maintainability concern with migration needs; moderate impact and manageable risk. |
| QAS-27 | Users misread "paid" vs "owes" semantics | High | Medium | No | Directly affects financial decisions; UX research and language design mitigate risk. |
| QAS-28 | Removed member accesses cached private reports | High | High | Yes | Privacy breach and authorization failure are severe business/legal risks with non-trivial cache/session invalidation. |
| QAS-29 | Long offline period followed by conflict-heavy sync | High | High | Yes | Mobile reality for shared expenses; merge/conflict design is complex and central to perceived reliability. |
| QAS-30 | Delayed/duplicate/out-of-order push events | Medium | Medium | No | Common distributed-system condition; correctness possible via dedupe + authoritative refresh model. |
| QAS-31 | Stolen device with active authenticated session | High | Medium | No | High security/privacy impact; controls (re-auth, revoke, session policy) are standard but essential. |
| QAS-32 | Android/iOS release skew causes split-logic divergence | High | High | Yes | Cross-platform financial parity is fundamental; release/version contract mistakes can create systemic inconsistencies. |
| QAS-33 | Poor observability for balance mismatch incidents | High | High | Yes | Without traceability, high-severity finance incidents persist longer and erode trust; architecture must support diagnosis. |

## Architectural Drivers (High Importance + High Risk)

- QAS-03: Invite abuse resistance and service resilience.
- QAS-10: Expense write-path performance at scale.
- QAS-13: Deterministic split math across platforms.
- QAS-15: Concurrency-safe expense editing.
- QAS-16: Ledger integrity with dependent settlements.
- QAS-18: Cross-device balance convergence.
- QAS-19: Debt overview scalability.
- QAS-20: Settlement deduplication under concurrency.
- QAS-28: Post-removal data access revocation.
- QAS-29: Offline-first sync conflict resolution.
- QAS-32: Cross-platform version compatibility/parity.
- QAS-33: Production observability for financial correctness.

## Decision Matrix Criteria

Derived from all architectural drivers plus scenarios with High business importance and Medium technical risk.

| Criterion | Why it matters? |
|---|---|
| Abuse-resistant invite and join protection (QAS-03) | Prevents unauthorized group access and protects uptime during attack or viral link leakage. |
| Scalable expense write latency (QAS-10) | Expense entry is the primary workflow; slow writes reduce adoption and trust. |
| Deterministic split and rounding correctness across platforms (QAS-13) | Financial results must be identical on Android, iOS, and backend to avoid disputes. |
| Concurrency-safe expense editing (QAS-15) | Prevents lost updates and inconsistent balances when multiple members edit simultaneously. |
| Ledger integrity with dependency-safe deletion/edit rules (QAS-16) | Protects core accounting invariants and avoids long-lived balance corruption. |
| Fast cross-device balance convergence (QAS-18) | Users compare balances across devices; divergence quickly erodes confidence in the product. |
| Scalable debt overview computation (QAS-19) | Large groups must still see "who owes whom" quickly for practical settlement decisions. |
| Settlement deduplication and idempotency (QAS-20) | Double-recorded payments cause direct financial errors and support burden. |
| Immediate access revocation for removed members (QAS-28) | Prevents privacy leaks and reduces legal/compliance exposure for shared financial data. |
| Deterministic offline sync conflict resolution (QAS-29) | Mobile users go offline frequently; conflict handling quality determines reliability perception. |
| Cross-platform release parity and backward compatibility (QAS-32) | Version skew must not change money calculations or break inter-client consistency. |
| End-to-end financial observability and traceability (QAS-33) | Critical incidents must be diagnosable quickly to restore trust and service health. |
| Reliable group creation under unstable network (QAS-02) | First-use success impacts onboarding conversion and early product confidence. |
| Controlled group exit with unsettled balances (QAS-05) | Enforces fair outcomes and prevents unresolved debt states in active groups. |
| Safe destructive actions for group deletion (QAS-07) | Prevents catastrophic accidental loss of shared financial history. |
| Duplicate-expense prevention for retries/double-taps (QAS-09) | Mobile retry behavior is common; dedupe protects ledger correctness. |
| Strict over-settlement validation and guardrails (QAS-21) | Blocks manipulation and high-impact input mistakes in payment flows. |
| Correct, stable history pagination and ordering (QAS-24) | Expense history is the audit trail; missing/duplicate rows undermine transparency. |
| Unambiguous money semantics in UI (paid vs owes) (QAS-27) | Users make real payment decisions from labels and signs; ambiguity causes wrong transfers. |
| Session hardening for stolen/lost devices (QAS-31) | Protects sensitive financial group data when device-level security fails. |

## Weighted Criteria (100-Point Allocation)

Weights are assigned from the QAW prioritization: financial correctness, security/privacy, and cross-device consistency are weighted highest because failure in these areas directly undermines trust and product viability.

| Criterion | Weight (points) |
|---|---:|
| Deterministic split and rounding correctness across platforms (QAS-13) | 8 |
| Ledger integrity with dependency-safe deletion/edit rules (QAS-16) | 8 |
| Fast cross-device balance convergence (QAS-18) | 7 |
| Settlement deduplication and idempotency (QAS-20) | 7 |
| Deterministic offline sync conflict resolution (QAS-29) | 7 |
| Immediate access revocation for removed members (QAS-28) | 6 |
| Cross-platform release parity and backward compatibility (QAS-32) | 6 |
| Scalable expense write latency (QAS-10) | 6 |
| Concurrency-safe expense editing (QAS-15) | 6 |
| Abuse-resistant invite and join protection (QAS-03) | 5 |
| Scalable debt overview computation (QAS-19) | 5 |
| End-to-end financial observability and traceability (QAS-33) | 4 |
| Duplicate-expense prevention for retries/double-taps (QAS-09) | 4 |
| Strict over-settlement validation and guardrails (QAS-21) | 4 |
| Session hardening for stolen/lost devices (QAS-31) | 4 |
| Reliable group creation under unstable network (QAS-02) | 3 |
| Correct, stable history pagination and ordering (QAS-24) | 3 |
| Unambiguous money semantics in UI (paid vs owes) (QAS-27) | 3 |
| Controlled group exit with unsettled balances (QAS-05) | 2 |
| Safe destructive actions for group deletion (QAS-07) | 2 |
| **Total** | **100** |

## Frontend Options Rating Matrix (1-5)

Scale:
- 1 = Poor
- 2 = Below average
- 3 = Adequate
- 4 = Good
- 5 = Excellent

Options:
- Flutter
- React Native + Expo
- Kotlin Multiplatform + Compose Multiplatform (KMP)
- Ionic + Capacitor

| Criterion | Flutter | React Native + Expo | KMP + Compose | Ionic + Capacitor |
|---|---:|---:|---:|---:|
| Deterministic split and rounding correctness across platforms (QAS-13) | 4 | 4 | 5 | 3 |
| Ledger integrity with dependency-safe deletion/edit rules (QAS-16) | 4 | 4 | 5 | 3 |
| Fast cross-device balance convergence (QAS-18) | 4 | 4 | 5 | 3 |
| Settlement deduplication and idempotency (QAS-20) | 4 | 4 | 5 | 3 |
| Deterministic offline sync conflict resolution (QAS-29) | 4 | 4 | 5 | 3 |
| Immediate access revocation for removed members (QAS-28) | 4 | 4 | 5 | 3 |
| Cross-platform release parity and backward compatibility (QAS-32) | 5 | 4 | 4 | 3 |
| Scalable expense write latency (QAS-10) | 4 | 4 | 4 | 3 |
| Concurrency-safe expense editing (QAS-15) | 4 | 4 | 5 | 3 |
| Abuse-resistant invite and join protection (QAS-03) | 4 | 4 | 4 | 3 |
| Scalable debt overview computation (QAS-19) | 4 | 4 | 4 | 3 |
| End-to-end financial observability and traceability (QAS-33) | 4 | 4 | 4 | 3 |
| Duplicate-expense prevention for retries/double-taps (QAS-09) | 4 | 4 | 5 | 3 |
| Strict over-settlement validation and guardrails (QAS-21) | 4 | 4 | 5 | 3 |
| Session hardening for stolen/lost devices (QAS-31) | 4 | 4 | 5 | 3 |
| Reliable group creation under unstable network (QAS-02) | 4 | 4 | 4 | 3 |
| Correct, stable history pagination and ordering (QAS-24) | 4 | 4 | 5 | 3 |
| Unambiguous money semantics in UI (paid vs owes) (QAS-27) | 4 | 4 | 4 | 3 |
| Controlled group exit with unsettled balances (QAS-05) | 4 | 4 | 5 | 3 |
| Safe destructive actions for group deletion (QAS-07) | 4 | 4 | 5 | 3 |

### Quick Read

- Flutter: strongest overall parity and UX/performance balance for one-codebase delivery.
- React Native + Expo: close to Flutter, slightly lower on parity risk due to dependency/native module drift.
- KMP + Compose: strongest for shared domain correctness and policy enforcement, with higher implementation complexity.
- Ionic + Capacitor: good for rapid web-style delivery, weaker for demanding offline/concurrency/performance requirements.

## Weighted Totals (Score x Weight)

Formula:
- Per criterion contribution = `rating (1-5) x criterion weight`
- Option total = sum of all criterion contributions
- Normalized score (0-5) = `option total / 100`

| Option | Weighted Total (max 500) | Normalized Score (0-5) |
|---|---:|---:|
| KMP + Compose | 468 | 4.68 |
| Flutter | 406 | 4.06 |
| React Native + Expo | 400 | 4.00 |
| Ionic + Capacitor | 300 | 3.00 |

Ranking:
1. KMP + Compose (468)
2. Flutter (406)
3. React Native + Expo (400)
4. Ionic + Capacitor (300)

## Backend Options Rating Matrix (1-5)

Scale:
- 1 = Poor
- 2 = Below average
- 3 = Adequate
- 4 = Good
- 5 = Excellent

Options:
- Spring Boot (Java/Kotlin)
- ASP.NET Core (C#)
- NestJS (Node.js/TypeScript)
- FastAPI (Python)

| Criterion | Spring Boot | ASP.NET Core | NestJS | FastAPI |
|---|---:|---:|---:|---:|
| Deterministic split and rounding correctness across platforms (QAS-13) | 5 | 5 | 4 | 4 |
| Ledger integrity with dependency-safe deletion/edit rules (QAS-16) | 5 | 5 | 4 | 4 |
| Fast cross-device balance convergence (QAS-18) | 4 | 4 | 4 | 4 |
| Settlement deduplication and idempotency (QAS-20) | 5 | 5 | 4 | 4 |
| Deterministic offline sync conflict resolution (QAS-29) | 5 | 5 | 4 | 4 |
| Immediate access revocation for removed members (QAS-28) | 5 | 5 | 4 | 4 |
| Cross-platform release parity and backward compatibility (QAS-32) | 5 | 5 | 4 | 4 |
| Scalable expense write latency (QAS-10) | 5 | 5 | 4 | 4 |
| Concurrency-safe expense editing (QAS-15) | 5 | 5 | 4 | 4 |
| Abuse-resistant invite and join protection (QAS-03) | 5 | 5 | 4 | 4 |
| Scalable debt overview computation (QAS-19) | 5 | 5 | 4 | 4 |
| End-to-end financial observability and traceability (QAS-33) | 5 | 5 | 4 | 4 |
| Duplicate-expense prevention for retries/double-taps (QAS-09) | 5 | 5 | 4 | 4 |
| Strict over-settlement validation and guardrails (QAS-21) | 5 | 5 | 4 | 4 |
| Session hardening for stolen/lost devices (QAS-31) | 5 | 5 | 4 | 4 |
| Reliable group creation under unstable network (QAS-02) | 4 | 4 | 4 | 4 |
| Correct, stable history pagination and ordering (QAS-24) | 5 | 5 | 4 | 4 |
| Unambiguous money semantics in UI (paid vs owes) (QAS-27) | 4 | 4 | 4 | 4 |
| Controlled group exit with unsettled balances (QAS-05) | 5 | 5 | 4 | 4 |
| Safe destructive actions for group deletion (QAS-07) | 5 | 5 | 4 | 4 |

### Quick Read

- Spring Boot: strongest match for transaction-heavy correctness, consistency, and security controls.
- ASP.NET Core: equally strong enterprise-grade fit, especially for performance and observability.
- NestJS: strong delivery speed and maintainability with good but less opinionated transaction guarantees.
- FastAPI: very productive and clear, but needs stricter architecture discipline for high-concurrency financial invariants.

## Backend Weighted Totals (Score x Weight)

Formula:
- Per criterion contribution = `rating (1-5) x criterion weight`
- Option total = sum of all criterion contributions
- Normalized score (0-5) = `option total / 100`

| Option | Weighted Total (max 500) | Normalized Score (0-5) |
|---|---:|---:|
| Spring Boot | 487 | 4.87 |
| ASP.NET Core | 487 | 4.87 |
| NestJS | 400 | 4.00 |
| FastAPI | 400 | 4.00 |

Ranking:
1. Spring Boot (487)
2. ASP.NET Core (487)
3. NestJS (400)
4. FastAPI (400)

Tie note:
- Spring Boot and ASP.NET Core are tied on weighted score.
- NestJS and FastAPI are tied on weighted score.

## Database Options Rating Matrix (1-5)

Scale:
- 1 = Poor
- 2 = Below average
- 3 = Adequate
- 4 = Good
- 5 = Excellent

Options:
- PostgreSQL
- MySQL 8 (InnoDB)
- CockroachDB
- MongoDB (Replica Set/Atlas)

| Criterion | PostgreSQL | MySQL 8 | CockroachDB | MongoDB |
|---|---:|---:|---:|---:|
| Deterministic split and rounding correctness across platforms (QAS-13) | 5 | 4 | 5 | 3 |
| Ledger integrity with dependency-safe deletion/edit rules (QAS-16) | 5 | 4 | 5 | 3 |
| Fast cross-device balance convergence (QAS-18) | 4 | 4 | 5 | 4 |
| Settlement deduplication and idempotency (QAS-20) | 5 | 4 | 5 | 3 |
| Deterministic offline sync conflict resolution (QAS-29) | 5 | 4 | 5 | 3 |
| Immediate access revocation for removed members (QAS-28) | 5 | 4 | 5 | 3 |
| Cross-platform release parity and backward compatibility (QAS-32) | 5 | 4 | 4 | 3 |
| Scalable expense write latency (QAS-10) | 5 | 4 | 4 | 4 |
| Concurrency-safe expense editing (QAS-15) | 5 | 4 | 5 | 3 |
| Abuse-resistant invite and join protection (QAS-03) | 4 | 4 | 4 | 3 |
| Scalable debt overview computation (QAS-19) | 5 | 4 | 4 | 4 |
| End-to-end financial observability and traceability (QAS-33) | 5 | 4 | 4 | 4 |
| Duplicate-expense prevention for retries/double-taps (QAS-09) | 5 | 4 | 5 | 3 |
| Strict over-settlement validation and guardrails (QAS-21) | 5 | 4 | 5 | 3 |
| Session hardening for stolen/lost devices (QAS-31) | 5 | 4 | 4 | 3 |
| Reliable group creation under unstable network (QAS-02) | 4 | 4 | 4 | 3 |
| Correct, stable history pagination and ordering (QAS-24) | 5 | 4 | 4 | 4 |
| Unambiguous money semantics in UI (paid vs owes) (QAS-27) | 4 | 4 | 4 | 4 |
| Controlled group exit with unsettled balances (QAS-05) | 5 | 4 | 5 | 3 |
| Safe destructive actions for group deletion (QAS-07) | 5 | 4 | 5 | 3 |

### Quick Read

- PostgreSQL: strongest single-node transactional fit for financial correctness and query flexibility.
- MySQL 8: reliable and mature relational option with slightly lower flexibility for advanced constraints/query patterns.
- CockroachDB: best for built-in distribution/availability with strong consistency, but more operational complexity.
- MongoDB: strong developer velocity and flexible schema, but weaker natural fit for strict ledger invariants.

## Database Weighted Totals (Score x Weight)

Formula:
- Per criterion contribution = `rating (1-5) x criterion weight`
- Option total = sum of all criterion contributions
- Normalized score (0-5) = `option total / 100`

| Option | Weighted Total (max 500) | Normalized Score (0-5) |
|---|---:|---:|
| PostgreSQL | 482 | 4.82 |
| CockroachDB | 461 | 4.61 |
| MySQL 8 | 400 | 4.00 |
| MongoDB | 328 | 3.28 |

Ranking:
1. PostgreSQL (482)
2. CockroachDB (461)
3. MySQL 8 (400)
4. MongoDB (328)
