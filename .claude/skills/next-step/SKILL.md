---
name: next-step
description: Work on the next step of PLAN.md in the agreed way, from explaining the change to showing the diff, then stop for review. Use when the developer asks to continue, start a step, or do the next task.
---

# Next step

Follow the workflow in `CLAUDE.md`. If a step number was given, use it. Otherwise take the first step in
`PLAN.md` that is not `done`.

1. Read the step and the code it touches. Set the step to `in progress`.
2. Explain in Macedonian, in a few sentences, what will change and why. Name any real choice with a
   recommendation, and wait for an answer before going on.
3. Make only the changes of this step. If something else looks wrong, mention it instead of fixing it.
4. For a bug fix, write the failing test first and show that it fails.
5. Build and run the tests. Report the real result, including failures.
6. Show the diff and go through it: what changed, why, and what to look at closely.
7. Stop and wait. Do not commit until the developer says so.

After the developer approves and the commit is made, mark the step `done` in `PLAN.md` and add any
decision or correction from this step to its log.
