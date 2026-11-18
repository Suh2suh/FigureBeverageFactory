[Code Convention]

Follow these conventions for all C# code in this project.

1. Naming
- Private fields: `_camelCase`
- Public fields and properties: `PascalCase`
- Private constants: `k_camelCase`
- Public constants: `k_PascalCase`
- Static members follow their access-level naming rule without extra prefixes.
- Names must express domain meaning and responsibility.
- Avoid vague names such as `data`, `info`, `item`, `obj`, `value`, and `temp`.
- Avoid project-specific abbreviations.
- Use common abbreviations only when their meaning is clear, such as `Id`, `Url`, and `Json`.

2. Methods and Parameters
- Methods use `PascalCase`.
- Method names must express the action and its target or scope.
- Parameter names must express the role of the passed value.
- Distinguish parameters with the same type by their semantic role.
  - Prefer: `StartConveyersInConnection()`, `processingUnit`, `currentOrder`
  - Avoid: `StartConnection()`, `unit`, `order`

3. Serialized Fields
- Private serialized fields follow private field naming.
- Prefer `[SerializeField] private _camelCase` fields for Inspector values.
- Use `[ReadOnly]` when a value should be visible but not manually editable.

4. Boolean Values
- Use `Is`, `Has`, `Can`, or `Should` prefixes when appropriate.
  - Examples: `IsInitialized`, `HasMaterial`, `CanStart`

5. Enums
- Enum type names and values use `PascalCase`.

6. Dictionary Naming
- Name dictionaries by semantic mapping:
  `keyMeaningToValueMeaning`
- Use meanings, not type names.
- Do not repeat redundant type suffixes.
  - Example: `Dictionary<int, OrderData> orderIdToOrder`
  - Example: `Dictionary<string, AnimalType> nameToAnimalType`

7. Unity
- Use standard Unity event method names:
  `Awake()`, `OnEnable()`, `Start()`, `Update()`, `OnDisable()`
- Prefer serialized fields over constants for Inspector-controlled values.
- Use constants only for values that must remain fixed.

8. Responsibility
- Separate shared responsibilities from domain-specific responsibilities.
- Keep Processing, Logistics, Data, and Control responsibilities independent.
- Do not make a class depend on functionality outside its responsibility.

9. Simplicity
- Implement only the requested behavior.
- Avoid unnecessary abstractions, defensive code, allocations, and future features.
- Prefer the simplest readable implementation that satisfies the intent.

10. Existing Structure
- Inspect the existing code and data flow before making changes.
- Preserve existing behavior and references unless the request requires changing them.
- Follow the existing project structure and style.

11. Collaboration
- Explain reasoning, assumptions, risks, and trade-offs when design decisions are involved.
- Do not respond defensively or agree automatically with feedback.
- Respectfully challenge assumptions when appropriate.
- The user owns final product decisions and trade-off choices.
