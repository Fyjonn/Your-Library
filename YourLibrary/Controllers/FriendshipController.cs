using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using YourLibrary.Data;
using YourLibrary.Models;

namespace YourLibrary.Controllers
{
    [Authorize]
    public class FriendshipController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FriendshipController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if(currentUser == null)
            {
                return RedirectToAction("Index","Home");
            }

            var model = new FriendshipPageViewModel();

            model.ReceivedInvitations = await _context.Friendships.Include(x => x.Requester)
                .Where(x=> x.ReceiverId == currentUser.Id && x.FriendStatus == EnumFriendStatus.Pending).ToListAsync();

            model.SentInvitations = await _context.Friendships.Include(x => x.Receiver)
                .Where(x => x.RequesterId == currentUser.Id && x.FriendStatus == EnumFriendStatus.Pending).ToListAsync();

            model.Friends = await _context.Friendships.Include(x => x.Requester).Include(x => x.Receiver)
                .Where(x => (x.RequesterId ==currentUser.Id || x.ReceiverId == currentUser.Id) && x.FriendStatus == EnumFriendStatus.Accepted).ToListAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SendInvitation(string searchUsername)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if(currentUser == null)
            {
                return RedirectToAction("Index");
            }

            var receiver = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == searchUsername);

            if(receiver == null)
            {
                TempData["FriendError"] = "User not found";
                return RedirectToAction("Index");
            }

            if(receiver.Id == currentUser.Id)
            {
                TempData["FriendError"] = "You cannt add yourself";
                return RedirectToAction("Index");
            }

            var existingFriendship = await _context.Friendships
                .FirstOrDefaultAsync(x => (x.RequesterId == currentUser.Id && x.Receiver.Id == receiver.Id) || (x.RequesterId == receiver.Id && x.ReceiverId == currentUser.Id));

            if(existingFriendship != null)
            {
                TempData["FriendError"] = "Friend request already exists.";
                return RedirectToAction("Index");
            }

            var friendship = new Friendship
            {
                RequesterId = currentUser.Id,
                ReceiverId = receiver.Id,
                FriendStatus = EnumFriendStatus.Pending
            };

            _context.Friendships.Add(friendship);

            await _context.SaveChangesAsync();

            TempData["FriendSuccess"] = "Invitation sent";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AcceptInvitation(int friendshipId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if(currentUser == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var friendship = await _context.Friendships.FirstOrDefaultAsync(x => x.FriendshipId == friendshipId && x.ReceiverId == currentUser.Id);

            if(friendship == null)
            {
                return RedirectToAction(nameof(Index));
            }

            friendship.FriendStatus = EnumFriendStatus.Accepted;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RejectInvitation(int friendshipId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var friendship = await _context.Friendships.FirstOrDefaultAsync(x => x.FriendshipId == friendshipId && x.ReceiverId == currentUser.Id);

            if (friendship == null)
            {
                return RedirectToAction(nameof(Index));
            }

            _context.Friendships.Remove(friendship);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteFriend(int friendshipId)
        {
            var currectUser = await _userManager.GetUserAsync(User);

            if(currectUser == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var friendship = await _context.Friendships.FirstOrDefaultAsync(x => x.FriendshipId == friendshipId && (x.RequesterId == currectUser.Id || x.ReceiverId == currectUser.Id));

            if(friendship == null)
            {
                return RedirectToAction(nameof(Index));
            }

            _context.Friendships.Remove(friendship);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CancelInvitation(int friendshipId)
        {
            var currectUser = await _userManager.GetUserAsync(User);

            if (currectUser == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var friendship = await _context.Friendships.FirstOrDefaultAsync(x => x.FriendshipId == friendshipId && x.RequesterId == currectUser.Id && x.FriendStatus == EnumFriendStatus.Pending);

            if (friendship == null)
            {
                return RedirectToAction(nameof(Index));
            }

            _context.Friendships.Remove(friendship);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
