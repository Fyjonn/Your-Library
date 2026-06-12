using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using YourLibrary.Data;
using YourLibrary.Models;

/// <summary>
/// Kontroler odpowiadajcy za zarzadzanie przyjazniami miedzy uzytkownikami. Zawiera obslugie zakladki "Friends",
/// w tym wyswietlanie zaproszen, listy przyjaciol, a takze odpowiada za logike wypozyczania ksiazek.
/// </summary>

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


        // zakladka friends
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if(currentUser == null)
            {
                return RedirectToAction("Index");
            }

            var model = new FriendshipPageViewModel();

            model.ReceivedInvitations = await _context.Friendships.Include(x => x.Requester)
                .Where(x=> x.ReceiverId == currentUser.Id && x.FriendStatus == EnumFriendStatus.Pending).ToListAsync();

            model.SentInvitations = await _context.Friendships.Include(x => x.Receiver)
                .Where(x => x.RequesterId == currentUser.Id && x.FriendStatus == EnumFriendStatus.Pending).ToListAsync();

            model.Friends = await _context.Friendships.Include(x => x.Requester).Include(x => x.Receiver)
                .Where(x => (x.RequesterId ==currentUser.Id || x.ReceiverId == currentUser.Id) && x.FriendStatus == EnumFriendStatus.Accepted).ToListAsync();

            model.IncomingBorrowRequests = await _context.Borrows.Include(b => b.ApplicationUser).Include(b => b.UserBook).ThenInclude(ub => ub.Book) 
                 .Where(b => b.UserBook.ApplicationUserId == currentUser.Id && b.StatusBorrow == EnumStatusBorrow.Requested).ToListAsync();

            model.MyBorrowedBooks = await _context.Borrows
            .Include(b => b.UserBook).ThenInclude(ub => ub.Book)
            .Include(b => b.UserBook).ThenInclude(ub => ub.ApplicationUser)
            .Where(b => b.ApplicationUserId == currentUser.Id && b.StatusBorrow == EnumStatusBorrow.Borrowed)
            .ToListAsync();

            // aktywne wypozyczone komus
            model.MyRentedBooks = await _context.Borrows
                .Include(b => b.ApplicationUser).Include(b => b.UserBook).ThenInclude(ub => ub.Book)
                .Where(b => b.UserBook.ApplicationUserId == currentUser.Id && b.StatusBorrow == EnumStatusBorrow.Borrowed)
                .ToListAsync();

            // historia - borrowed books
            model.MyBorrowedHistory = await _context.Borrows
                .Include(b => b.UserBook).ThenInclude(ub => ub.Book)
                .Include(b => b.UserBook).ThenInclude(ub => ub.ApplicationUser)
                .Where(b => b.ApplicationUserId == currentUser.Id && b.StatusBorrow == EnumStatusBorrow.Completed)
                .OrderByDescending(b => b.ReturnDate)
                .ToListAsync();

            //historia - rented books
            model.MyRentedHistory = await _context.Borrows
                .Include(b => b.ApplicationUser).Include(b => b.UserBook).ThenInclude(ub => ub.Book)
                .Where(b => b.UserBook.ApplicationUserId == currentUser.Id && b.StatusBorrow == EnumStatusBorrow.Completed)
                .OrderByDescending(b => b.ReturnDate)
                .ToListAsync();

            // potwierdzanie zwrotow
            ViewBag.ReturnsToConfirm = await _context.Borrows
                .Include(b => b.ApplicationUser).Include(b => b.UserBook).ThenInclude(ub => ub.Book)
                .Where(b => b.UserBook.ApplicationUserId == currentUser.Id && b.StatusBorrow == EnumStatusBorrow.Returned)
                .ToListAsync();

            return View(model);
        }

        // pozyczanie ksiazek - zapytanie
        [HttpPost]
        public async Task<IActionResult> RequestBorrow(int userBookId, string friendName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index");
            }

            var alreadyRequested = await _context.Borrows
                .AnyAsync(b => b.UserBookId == userBookId && b.ApplicationUserId == currentUser.Id && b.StatusBorrow == EnumStatusBorrow.Requested);

            if (!alreadyRequested)
            {
                var borrowRequest = new Borrow
                {
                    UserBookId = userBookId,
                    ApplicationUserId = currentUser.Id,
                    StatusBorrow = EnumStatusBorrow.Requested,
                    BorrowDate = DateTime.Now,
                    ReturnDate = DateTime.Now.AddDays(30) 
                };

                _context.Borrows.Add(borrowRequest);
                await _context.SaveChangesAsync();
                TempData["FriendSuccess"] = "Borrow request sent successfully!";
            }
            else
            {
                TempData["FriendError"] = "You have already requested this book.";
            }

            return RedirectToAction("Shelf", new { friendName = friendName });
        }

        // pozyczanie ksiazek - akceptacja
        [HttpPost]
        public async Task<IActionResult> AcceptBorrow(int borrowId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var borrow = await _context.Borrows
                .Include(b => b.UserBook)
                .FirstOrDefaultAsync(b => b.BorrowId == borrowId && b.UserBook.ApplicationUserId == currentUser.Id);

            if (borrow != null && borrow.UserBook != null)
            {
                borrow.OriginalOwnerReadStatus = borrow.UserBook.ReadStatus;
                borrow.StatusBorrow = EnumStatusBorrow.Borrowed;
                borrow.BorrowDate = DateTime.Now;
                borrow.UserBook.IsBorrowed = true;
                borrow.UserBook.ReadStatus = EnumReadStatus.ToRead;

                await _context.SaveChangesAsync();

                TempData["FriendSuccess"] = "You have accepted the borrow request!";
            }
            else
            {
                TempData["FriendError"] = "Borrow request not found or you are not authorized.";
            }

            return RedirectToAction(nameof(Index));
        }

        // pozyczanie ksiazek - odrzucenie
        [HttpPost]
        public async Task<IActionResult> RejectBorrow(int borrowId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction(nameof(Index));


            var borrow = await _context.Borrows
                .Include(b => b.UserBook)
                .FirstOrDefaultAsync(b => b.BorrowId == borrowId && b.UserBook.ApplicationUserId == currentUser.Id);

            if (borrow != null)
            {
                _context.Borrows.Remove(borrow);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // wyslanie zaproszenia
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

        // akceptacja zaproszenia
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

        //odrzucenie zaproszenia
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

        // usuwanie znajomego
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

        // anulowanie zaproszenia
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

        // podglad polki znajomego przy akcji wypozyczania ksiazki
        [HttpGet]
        public async Task<IActionResult> Shelf(string friendName)
        {
            if (string.IsNullOrEmpty(friendName))
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index");
            }

            var friendUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == friendName);
            if (friendUser == null)
            {
                return NotFound();
            }

            var books = await _context.UserBooks
                .Include(ub => ub.Book)
                .Where(ub => ub.ApplicationUserId == friendUser.Id)
                .ToListAsync();

            var currentlyBorrowedIds = await _context.Borrows
                .Where(b => b.StatusBorrow == EnumStatusBorrow.Borrowed || b.StatusBorrow == EnumStatusBorrow.Returned)
                .Select(b => b.UserBookId)
                .ToListAsync();
            var OwnedBooksIds = await _context.UserBooks.Where(ub => ub.ApplicationUserId == friendUser.Id && ub.IsOwned == true)
                .Select(ub => ub.UserBookId)
                .ToListAsync();

            var availableBooks = books.Where(ub => !currentlyBorrowedIds.Contains(ub.UserBookId) && OwnedBooksIds.Contains(ub.UserBookId)).ToList();

            var viewModel = new FriendShelfViewModel
            {
                FriendName = friendName,
                Books = availableBooks
            };

            return View(viewModel);
        }


        // zwracanie ksiazki
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBorrowedBookDirectly(int userBookId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Index");

            var borrowRecord = await _context.Borrows
                .Include(b => b.UserBook)
                .FirstOrDefaultAsync(b => b.UserBookId == userBookId
                                       && b.ApplicationUserId == currentUser.Id
                                       && b.StatusBorrow == EnumStatusBorrow.Borrowed);

            if (borrowRecord == null)
            {
                TempData["FriendError"] = "Borrow record not found.";
                return RedirectToAction("Index");
            }

            borrowRecord.BorrowerFinalReadStatus = borrowRecord.UserBook.ReadStatus;
            borrowRecord.StatusBorrow = EnumStatusBorrow.Returned;
            borrowRecord.ReturnDate = DateTime.Now;

            _context.Entry(borrowRecord).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            TempData["FriendSuccess"] = "Book returned successfully!";
            return RedirectToAction("Index");
        }

        // potwierdznie zwrotu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReturn(int borrowId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction(nameof(Index));

            var borrow = await _context.Borrows
                .Include(b => b.UserBook)
                .FirstOrDefaultAsync(b => b.BorrowId == borrowId && b.UserBook.ApplicationUserId == currentUser.Id);

            if (borrow != null && borrow.StatusBorrow == EnumStatusBorrow.Returned)
            {
                borrow.StatusBorrow = EnumStatusBorrow.Completed;
                borrow.UserBook.IsBorrowed = false;
                borrow.UserBook.Location = null;
                borrow.UserBook.Notes = null;
                borrow.UserBook.ReadStatus = borrow.OriginalOwnerReadStatus;

                _context.Entry(borrow).State = EntityState.Modified;
                _context.Entry(borrow.UserBook).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                TempData["FriendSuccess"] = "Return confirmed and moved to history.";
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
