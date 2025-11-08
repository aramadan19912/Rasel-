export interface MessageRule {
  id: number;
  name: string;
  isEnabled: boolean;
  priority: number;
  conditions: RuleCondition[];
  actions: RuleAction[];
}

export interface RuleCondition {
  field: string; // Subject, From, To, Body, etc.
  operator: string; // Contains, Equals, StartsWith, etc.
  value: string;
  logicalOperator: LogicalOperator;
}

export interface RuleAction {
  actionType: string; // Move, Copy, Delete, Flag, Category, etc.
  parameters: { [key: string]: any };
}

export enum LogicalOperator {
  And = 0,
  Or = 1
}

export interface CreateRuleDto {
  userId: string;
  name: string;
  priority: number;
  conditions: RuleCondition[];
  actions: RuleAction[];
}
