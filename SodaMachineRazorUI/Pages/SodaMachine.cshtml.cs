using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SodaMachineLibrary.Logic;
using SodaMachineLibrary.Models;

namespace SodaMachineRazorUI
{
    [Authorize]
    public class SodaMachineModel : PageModel
    {
        private readonly ISodaMachineLogic _sodaMachine;

        public decimal SodaPrice { get; set; }
        public decimal DepositedAmount { get; set; }
        public List<SodaModel> SodaOptions { get; set; }

        [BindProperty]
        public decimal Deposit { get; set; }

        [BindProperty]
        public SodaModel SelectedSoda { get; set; }

        //[BindProperty(SupportsGet = true)]
        public string UserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string OutputText { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ErrorMessage { get; set; }

        public SodaMachineModel(ISodaMachineLogic sodaMachine)
        {
            _sodaMachine = sodaMachine;
        }

        public void OnGet()
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                //UserId = Guid.NewGuid().ToString();
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            SodaPrice = _sodaMachine.GetSodaPrice();
            DepositedAmount = _sodaMachine.GetMoneyInsertedTotal(UserId);
            SodaOptions = _sodaMachine.ListTypesOfSoda();
        }

        // Used for depositing coins
        public IActionResult OnPost()
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Deposit > 0)
            {
                _sodaMachine.MoneyInserted(UserId, Deposit);
            }

            //return RedirectToPage(new { UserId });
            return RedirectToPage();
        }

        // Used for requesting soda
        public IActionResult OnPostSoda()
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var results = _sodaMachine.RequestSoda(SelectedSoda, UserId);
            OutputText = "";

            // Handle the response
            if (results.errorMessage.Length > 0)
            {
                ErrorMessage = results.errorMessage;
            }
            else
            {
                OutputText = $"Here is your { results.soda.Name }";

                if (results.change.Count > 0)
                {
                    OutputText += "<br><br>Here is your change:<br>";
                    results.change.ForEach(x => OutputText += $"{ x.Name }<br>");
                }
                else
                {
                    OutputText += "<br><br>You used exact change so there is no change to refund.";
                }
            }

            //return RedirectToPage(new { UserId, ErrorMessage, OutputText });
            return RedirectToPage(new { ErrorMessage, OutputText });
        }

        // Used for cancelling our deposit
        public IActionResult OnPostCancel()
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DepositedAmount = _sodaMachine.GetMoneyInsertedTotal(UserId);
            _sodaMachine.IssueFullRefund(UserId);

            OutputText = $"You have been refunded { String.Format("{0:C}", DepositedAmount) }";

            //return RedirectToPage(new { UserId, OutputText });
            return RedirectToPage(new { OutputText });
        }
    }
}