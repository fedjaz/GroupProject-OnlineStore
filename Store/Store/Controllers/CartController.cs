﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Entities;
using System.Data.SqlClient;
using Store.Models;

namespace Store.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class CartController : Controller
    {
        private ApplicationDbContext _context;
        private UserManager<User> _userManager;
        public CartController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize (Roles = "admin")]
        public async Task<ActionResult<UserItem>> PostUserItem(UserItem userItem)
        {
            _context.UserItems.Add(userItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(PostUserItem), new { id = userItem.ID }, userItem);
        }

        [HttpGet]
        [Authorize (Roles="admin")]
        public async Task<ActionResult<IEnumerable<CartItemData>>> GetUserItems()
        {
            List<CartItemData> ans = new List<CartItemData>();
            foreach (var userItem in _context.UserItems)
            {
                ans.Add(new CartItemData
                {
                    ItemID = userItem.ID,
                    Count = userItem.Count,
                });
            }
            return ans;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<CartItemData>> GetUserItem(int id)
        {
            var userItem = await _context.UserItems.FindAsync(id);

            if (userItem == null)
            {
                return NotFound();
            }

            return new CartItemData
            {
                ItemID = userItem.ID,
                Count = userItem.Count,
            };
        }

        [HttpPost]
        [Authorize(Roles = "admin, user")]
        public async Task<IActionResult> AddItemForUser([FromForm] CartItemModel userItemModel)
        {
            User requestUser = await _userManager.FindByEmailAsync(User.Identity.Name);
            User targetUser = await _userManager.FindByIdAsync(userItemModel.UserID);
            CartItemResult userItemResult = new CartItemResult() { Success = true };

            if (string.IsNullOrEmpty(userItemModel.UserID))
            {
                targetUser = await _userManager.FindByIdAsync(requestUser.Id);
            }

            if (targetUser == null)
            {
                userItemResult.ErrorCodes.Add(CartItemResultConstants.ERROR_USER_INVALID);
            }
            else if (requestUser.Id != targetUser.Id && !await _userManager.IsInRoleAsync(requestUser, "admin"))
            {
                userItemResult.ErrorCodes.Add(CartItemResultConstants.ERROR_ACCESS_DENIED);
            }

            if (userItemResult.ErrorCodes.Count > 0)
            {
                userItemResult.Success = false;
                return Json(userItemResult);
            }

            if (userItemModel.Count < 1)
            {
                userItemResult.ErrorCodes.Add(CartItemResultConstants.ERROR_COUNT_LESS_ONE);
            }

            if (userItemResult.ErrorCodes.Count > 0)
            {
                userItemResult.Success = false;
                return Json(userItemResult);
            }

            UserItem userItem = new UserItem() 
            { 
                UserID = userItemModel.UserID,
                ItemID = userItemModel.ItemID,
                Count = userItemModel.Count
            };
            _context.Add(userItem);
            await _context.SaveChangesAsync();

            return Json(userItemResult);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> IncrementItemCount(int userId, int itemId)
        {
            User requestUser = await _userManager.FindByEmailAsync(User.Identity.Name);
            User targetUser = await _userManager.FindByIdAsync(userId.ToString());
            CartItemResult userItemResult = new CartItemResult() { Success = true };
            if (targetUser == null)
            {
                userItemResult.ErrorCodes.Add(CartItemResultConstants.ERROR_USER_INVALID);
            }
            else if (requestUser.Id != targetUser.Id && !await _userManager.IsInRoleAsync(requestUser, "admin"))
            {
                userItemResult.ErrorCodes.Add(CartItemResultConstants.ERROR_ACCESS_DENIED);
            }

            if (userItemResult.ErrorCodes.Count > 0)
            {
                userItemResult.Success = false;
                return Json(userItemResult);
            }

            UserItem? item = _context.UserItems.FirstOrDefault(item => item.ItemID == itemId
                                                                && item.UserID == userId.ToString());

            if (item == null)
            {
                userItemResult.ErrorCodes.Add(CartItemResultConstants.ERROR_CART_ITEM_NOT_FOUND);
            }

            if (userItemResult.ErrorCodes.Count > 0)
            {
                userItemResult.Success = false;
                return Json(userItemResult);
            }

            item.Count++;
            await _context.SaveChangesAsync();

            return Json(userItemResult);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DecrementItemCount(int userId, int itemId)
        {
            User requestUser = await _userManager.FindByEmailAsync(User.Identity.Name);
            User targetUser = await _userManager.FindByIdAsync(userId.ToString());
            CartItemResult userItemResult = new CartItemResult() { Success = true };
            if (targetUser == null)
            {
                userItemResult.ErrorCodes.Add(CartItemResultConstants.ERROR_USER_INVALID);
            }
            else if (requestUser.Id != targetUser.Id && !await _userManager.IsInRoleAsync(requestUser, "admin"))
            {
                userItemResult.ErrorCodes.Add(CartItemResultConstants.ERROR_ACCESS_DENIED);
            }

            if (userItemResult.ErrorCodes.Count > 0)
            {
                userItemResult.Success = false;
                return Json(userItemResult);
            }

            UserItem? item = _context.UserItems.FirstOrDefault(item => item.ItemID == itemId
                                                                && item.UserID == userId.ToString());

            if (item == null)
            {
                userItemResult.ErrorCodes.Add(CartItemResultConstants.ERROR_CART_ITEM_NOT_FOUND);
            }
            else if(item.Count == 1)
            {
                userItemResult.ErrorCodes.Add(CartItemResultConstants.ERROR_COUNT_LESS_ONE);
            }

            if (userItemResult.ErrorCodes.Count > 0)
            {
                userItemResult.Success = false;
                return Json(userItemResult);
            }

            item.Count--;
            await _context.SaveChangesAsync();

            return Json(userItemResult);
        }

        /// <summary>
        /// Удалить useritem
        /// </summary>
        /// <param name="id">id удаляемого useritem</param>      
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Remove(int id)
        {
            var userItem = _context.UserItems.Find(id);
            if (userItem == null)
            {
                return NotFound();
            }

            _context.UserItems.Remove(userItem);
            await _context.SaveChangesAsync();
            return NoContent();

        }

        [HttpGet]
        public async Task<IActionResult> GetShoppingDetails()
        {

            User user = await _userManager.GetUserAsync(User);
            List<UserItem> itemRelations = _context.UserItems.Where(userItem => userItem.UserID == user.Id).ToList();
            return Json(itemRelations);
        }

        [HttpGet]
        public async Task<IActionResult> GetTotal()
        {
            User user = await _userManager.GetUserAsync(User);
            
            decimal? total = decimal.Zero;
            total = (decimal?)(from cartItems in _context.UserItems
                               where cartItems.UserID == user.Id
                               join item in _context.Items on cartItems.ItemID equals item.ID
                   
                               select cartItems.Count 
                               *item.GetDiscountPrice(user.Discount)).Sum();
            return Json(total ?? decimal.Zero);
        }

    }
}
