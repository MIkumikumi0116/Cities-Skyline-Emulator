using ColossalFramework;
using ColossalFramework.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Emulator_Backend
{
    public static class EconomyHelper
    {
        public static string GetEconomyInfo()
        {
            var economyManager = Singleton<EconomyManager>.instance;

            var cash = economyManager.LastCashAmount;
            var income = economyManager.LastCashDelta;

            cash = Util.GetValue(cash, 100);
            income = Util.GetValue(income, 100);

            var json = Util.ConvertToJSON<object>(new Dictionary<object, object>
            {
                { "cash", cash },
                { "income", income }
            });

            return json;
        }

        public static string GetExpenses()
        {
            EconomyPanel economyPanel = GameObject.FindObjectOfType<EconomyPanel>();

            var policiesExpenses = economyPanel.expensesPoliciesTotal;
            var loansExpenses = economyPanel.expensesLoansTotal;

            string pattern = "[^0-9.]"; // 匹配任何非数字和非小数点的字符
            policiesExpenses = Regex.Replace(policiesExpenses, pattern, "");
            loansExpenses = Regex.Replace(loansExpenses, pattern, "");

            var basicExpensesPolls = economyPanel.GetType().GetField("basicExpensesPolls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(economyPanel) as IncomeExpensesPoll[];

            double total = 0;
            var dict = new Dictionary<object, object>
            {
                { "policiesExpenses", policiesExpenses },
                { "loansExpenses", loansExpenses }
            };

            total += double.Parse(policiesExpenses);
            total += double.Parse(loansExpenses);

            for (int i = 0; i < basicExpensesPolls.Length; i++)
            {
                var expensesFieldName = basicExpensesPolls[i].GetType().GetField("m_ExpensesFieldName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(basicExpensesPolls[i]) as string;
                if (expensesFieldName != null && !dict.ContainsKey(expensesFieldName))
                {
                    var expenses = Regex.Replace(basicExpensesPolls[i].expensesString, pattern, "");
                    if (string.IsNullOrEmpty(expenses))
                    {
                        expenses = "0";
                    }
                    dict.Add(expensesFieldName, expenses);
                    total += double.Parse(expenses);
                }
            }

            var publicTransportDetailExpensesPolls = economyPanel.GetType().GetField("publicTransportDetailExpensesPolls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(economyPanel) as IncomeExpensesPoll[];
            
            for (int i = 0; i < publicTransportDetailExpensesPolls.Length; i++)
            {
                ItemClass.SubService[] subServices = publicTransportDetailExpensesPolls[i].GetType().GetField("m_SubService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportDetailExpensesPolls[i]) as ItemClass.SubService[];
                if (subServices != null && subServices.Length > 0 && !dict.ContainsKey(subServices[0].ToString()))
                {
                    var expenses = Regex.Replace(publicTransportDetailExpensesPolls[i].expensesString, pattern, "");
                    if (string.IsNullOrEmpty(expenses))
                    {
                        expenses = "0";
                    }
                    dict.Add(subServices[0].ToString(), expenses);
                    total += double.Parse(expenses);
                }
            }

            dict.Add("total", total);
            var json = Util.ConvertToJSON<object>(dict);

            return json;
        }
    
        public static string GetIncomeInfo()
        {
            EconomyPanel economyPanel = GameObject.FindObjectOfType<EconomyPanel>();
            string pattern = "[^0-9.]"; // 匹配任何非数字和非小数点的字符
            Dictionary<object, object> dict = new Dictionary<object, object>();

            var basicIncomePolls = economyPanel.GetType().GetField("basicIncomePolls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(economyPanel) as IncomeExpensesPoll[];
            for (int i = 0; i < basicIncomePolls.Length; i++)
            {
                var incomeFieldName = basicIncomePolls[i].GetType().GetField("m_IncomeFieldName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(basicIncomePolls[i]) as string;
                if (incomeFieldName != null && !dict.ContainsKey(incomeFieldName))
                {
                    var income = Regex.Replace(basicIncomePolls[i].incomeString, pattern, "");
                    if (string.IsNullOrEmpty(income))
                    {
                        income = "0";
                    }
                    dict.Add(incomeFieldName, income);
                }
            }

            var residentialDetailIncomePolls = economyPanel.GetType().GetField("residentialDetailIncomePolls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(economyPanel) as IncomeExpensesPoll[];
            for (int i = 0; i < residentialDetailIncomePolls.Length; i++)
            {
                var incomeFieldName = residentialDetailIncomePolls[i].GetType().GetField("m_IncomeFieldName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(residentialDetailIncomePolls[i]) as string;
                if (incomeFieldName != null && !dict.ContainsKey("Residential" + incomeFieldName))
                {
                    var income = Regex.Replace(residentialDetailIncomePolls[i].incomeString, pattern, "");
                    if (string.IsNullOrEmpty(income))
                    {
                        income = "0";
                    }
                    dict.Add("Residential" + incomeFieldName, income);
                }
            }

            var publicTransportDetailIncomePolls = economyPanel.GetType().GetField("publicTransportDetailIncomePolls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(economyPanel) as IncomeExpensesPoll[];
            for (int i = 0; i < publicTransportDetailIncomePolls.Length; i++)
            {
                ItemClass.SubService[] subServices = publicTransportDetailIncomePolls[i].GetType().GetField("m_SubService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportDetailIncomePolls[i]) as ItemClass.SubService[];
                if (subServices != null && subServices.Length > 0 && !dict.ContainsKey(subServices[0].ToString()))
                {
                    var income = Regex.Replace(publicTransportDetailIncomePolls[i].incomeString, pattern, "");
                    if (string.IsNullOrEmpty(income))
                    {
                        income = "0";
                    }
                    dict.Add(subServices[0].ToString(), income);
                }
            }

            var commercialDetailIncomePolls = economyPanel.GetType().GetField("commercialDetailIncomePolls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(economyPanel) as IncomeExpensesPoll[];
            for (int i = 0; i < commercialDetailIncomePolls.Length; i++)
            {
                var incomeFieldName = commercialDetailIncomePolls[i].GetType().GetField("m_IncomeFieldName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(commercialDetailIncomePolls[i]) as string;
                if (incomeFieldName != null && !dict.ContainsKey("Commercial" + incomeFieldName))
                {
                    var income = Regex.Replace(commercialDetailIncomePolls[i].incomeString, pattern, "");
                    if (string.IsNullOrEmpty(income))
                    {
                        income = "0";
                    }
                    dict.Add("Commercial" + incomeFieldName, income);
                }
            }

            var industrialDetailIncomePolls = economyPanel.GetType().GetField("industrialDetailIncomePolls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(economyPanel) as IncomeExpensesPoll[];
            for (int i = 0; i < industrialDetailIncomePolls.Length; i++)
            {
                var incomeFieldName = industrialDetailIncomePolls[i].GetType().GetField("m_IncomeFieldName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(industrialDetailIncomePolls[i]) as string;
                if (incomeFieldName != null && !dict.ContainsKey("Industrial" + incomeFieldName))
                {
                    var income = Regex.Replace(industrialDetailIncomePolls[i].incomeString, pattern, "");
                    if (string.IsNullOrEmpty(income))
                    {
                        income = "0";
                    }
                    dict.Add("Industrial" + incomeFieldName, income);
                }
            }

            var officeDetailIncomePolls = economyPanel.GetType().GetField("officeDetailIncomePolls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(economyPanel) as IncomeExpensesPoll[];
            for (int i = 0; i < officeDetailIncomePolls.Length; i++)
            {
                var incomeFieldName = officeDetailIncomePolls[i].GetType().GetField("m_IncomeFieldName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(officeDetailIncomePolls[i]) as string;
                if (incomeFieldName != null && !dict.ContainsKey("Office" + incomeFieldName))
                {
                    var income = Regex.Replace(officeDetailIncomePolls[i].incomeString, pattern, "");
                    if (string.IsNullOrEmpty(income))
                    {
                        income = "0";
                    }
                    dict.Add("Office" + incomeFieldName, income);
                }
            }

            return Util.ConvertToJSON<object>(dict);
        }
    
        public static string GetBudgetInfo()
        {
            Dictionary<object, object> dict = new Dictionary<object, object>();

            EconomyPanel economyPanel = GameObject.FindObjectOfType<EconomyPanel>();
            UIComponent servicesBudgetContainer = economyPanel.component.Find("ServicesBudgetContainer");
            for (int i = 0; i < servicesBudgetContainer.components.Count; i++)
            {
                UIComponent component = servicesBudgetContainer.components[i];
                BudgetItem budgetItem = component.GetComponent<BudgetItem>();
                var service = (ItemClass.Service)budgetItem.GetType().GetField("m_Service", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(budgetItem);
                var totalLabel = budgetItem.GetType().GetField("m_TotalLabel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(budgetItem) as UILabel;
                var total = Regex.Replace(totalLabel.text, "[^0-9.]", "");
                var daylabel = budgetItem.GetType().GetField("m_DayPercentageLabel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(budgetItem) as UILabel;
                var nightlabel = budgetItem.GetType().GetField("m_NightPercentageLabel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(budgetItem) as UILabel;
                dict.Add(service.ToString() + "-day", daylabel.text);
                dict.Add(service.ToString() + "-night", nightlabel.text);
                dict.Add(service.ToString(), total);
            }

            UIComponent subServicesBudgetContainer = economyPanel.component.Find("SubServicesBudgetContainer");
            for (int i = 0; i < subServicesBudgetContainer.components.Count; i++)
            {
                UIComponent component = subServicesBudgetContainer.components[i];
                BudgetItem budgetItem = component.GetComponent<BudgetItem>();
                var service = (ItemClass.Service)budgetItem.GetType().GetField("m_Service", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(budgetItem);
                var subService = (ItemClass.SubService)budgetItem.GetType().GetField("m_SubService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(budgetItem);
                var totalLabel = budgetItem.GetType().GetField("m_TotalLabel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(budgetItem) as UILabel;
                var total = Regex.Replace(totalLabel.text, "[^0-9.]", "");
                var daylabel = budgetItem.GetType().GetField("m_DayPercentageLabel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(budgetItem) as UILabel;
                var nightlabel = budgetItem.GetType().GetField("m_NightPercentageLabel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(budgetItem) as UILabel;
                dict.Add(service.ToString() + "-" + subService.ToString() + "-day", daylabel.text);
                dict.Add(service.ToString() + "-" + subService.ToString() + "-night", nightlabel.text);
                dict.Add(service.ToString() + "-" + subService.ToString(), total);
            }

            return Util.ConvertToJSON<object>(dict);
        }
    }
}
