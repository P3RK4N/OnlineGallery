var searchData=
[
  ['mapget_0',['mapget',['../_program_8cs.html#ab688d13d545b936a2774fd43c0f2f578',1,'MapGet(&quot;/&quot;, async(HttpContext context, UserManager userManager)=&gt; { if(context.User.Identity==null||!context.User.Identity.IsAuthenticated) { return &quot;[Anonymous] Home&quot;;} var user=await userManager.GetUserAsync(context.User);return user==null ? &quot;[Anonymous] Home&quot; :$&quot;[{user.UserName}] Home&quot;;}):&#160;Program.cs'],['../_program_8cs.html#a56b2e9dceab4023c4f1c9fe69608fe33',1,'MapGet(&quot;/user/login&quot;,()=&gt; &quot;Hello login!&quot;):&#160;Program.cs'],['../_program_8cs.html#aa89a6f7a6c9be7e31f9e63413364273c',1,'MapGet(&quot;/user/register&quot;,()=&gt; &quot;Hello register!&quot;):&#160;Program.cs'],['../_program_8cs.html#aa5f65acedf592f7ee86ef26cb6ddb217',1,'MapGet(&quot;/user&quot;, async(HttpContext context, UserManager userManager)=&gt; { if(context.User.Identity==null||!context.User.Identity.IsAuthenticated) { context.Response.Redirect(&quot;/user/login&quot;);return;} var user=await userManager.GetUserAsync(context.User);if(user==null) { context.Response.Redirect(&quot;/user/login&quot;);return;} context.Response.Redirect($&quot;/users/{user.UserName}&quot;);}):&#160;Program.cs'],['../_program_8cs.html#a8df212bda1108c6c77af0004a27a9131',1,'MapGet(&quot;/users&quot;, async(HttpContext context, UserManager userManager)=&gt; { var users=await userManager.Users.ToListAsync();await context.Response.WriteAsJsonAsync(users);}):&#160;Program.cs'],['../_program_8cs.html#afd0af5fa5cc8a8d60b2e1ccbd7ccc6c9',1,'MapGet(&quot;/users/{username}&quot;, async(string username, HttpContext context, UserManager userManager)=&gt; { var users=await userManager.Users.Where(u=&gt; u.UserName==username).ToListAsync();if(users.Count==0) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync($&quot;User {username} does not exist!&quot;);return;} await context.Response.WriteAsJsonAsync(users[0]);}):&#160;Program.cs'],['../_program_8cs.html#a77287ade3bfe9343303ed0e1148f3079',1,'MapGet(&quot;/users/{username}/photos&quot;, async(string username, HttpContext context, UserManager userManager, DbContext dbContext)=&gt; { var users=await userManager.Users.Where(u=&gt; u.UserName==username).ToListAsync();if(users.Count==0) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync($&quot;User {username} does not exist!&quot;);return;} var photos=dbContext.Pictures.Where(p=&gt; p.UserId==users[0].Id).ToList().ConvertAll(p=&gt; new PhotoDTO { Id=p.Id, userId=p.UserId, content=p.Content, contentType=p.ContentType, fileName=p.FileName });await context.Response.WriteAsJsonAsync(photos);}):&#160;Program.cs'],['../_program_8cs.html#a1f40c2c8a9812b3e84528a9944763c31',1,'MapGet(&quot;/photos&quot;, async(HttpContext context, DbContext dbContext)=&gt; { var photos=dbContext.Pictures.ToList().ConvertAll(p=&gt; new PhotoDTO { Id=p.Id, userId=p.UserId, content=p.Content, contentType=p.ContentType, fileName=p.FileName });await context.Response.WriteAsJsonAsync(photos);}):&#160;Program.cs'],['../_program_8cs.html#a8ee9fbe9ac10789e06a218e70d2d1227',1,'MapGet(&quot;/photos/{photoID}&quot;, async(int photoID, HttpContext context, DbContext dbContext)=&gt; { var photo=await dbContext.Pictures.Where(p=&gt; p.Id==photoID).ToListAsync();if(photo.Count==0) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync($&quot;Photo with id: {photoID} does not exist!&quot;);return;} using(var ms=new MemoryStream(photo[0].Content)) { context.Response.ContentType=photo[0].ContentType;await ms.CopyToAsync(context.Response.Body);} }):&#160;Program.cs'],['../_program_8cs.html#a3771d369156388267bf09d253b984138',1,'MapGet(&quot;/photos/{photoID}/user&quot;, async(int photoID, HttpContext context, DbContext dbContext)=&gt; { var photoUserId=await dbContext.Pictures.Where(p=&gt; p.Id==photoID).Select(p=&gt; p.UserId).ToListAsync();if(photoUserId.Count==0) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync($&quot;Photo with id: {photoID} does not exist!&quot;);return;} var photoUser=await dbContext.Users.Where(u=&gt; u.Id==photoUserId[0]).ToListAsync();await context.Response.WriteAsJsonAsync(photoUser);}):&#160;Program.cs']]],
  ['mappost_1',['mappost',['../_program_8cs.html#a0bb68c1364de4dba6b1620966d499d54',1,'MapPost(&quot;/user/register&quot;, async(HttpContext context, RoleManager roleManager, UserManager userManager)=&gt; { var username=context.Request.Form[&quot;username&quot;];var password=context.Request.Form[&quot;password&quot;];if(StringValues.IsNullOrEmpty(username)||StringValues.IsNullOrEmpty(password)) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync(&quot;Registration failed!&quot;);return;} var user=new User { UserName=username };var result=await userManager.CreateAsync(user, password);if(result.Succeeded) { await context.Response.WriteAsync(&quot;User Registered Successfully!&quot;);} else { context.Response.StatusCode=400;await context.Response.WriteAsync(&quot;User registration failed:\n\t - &quot;+string.Join(&quot;\n\t - &quot;, result.Errors.Select(e=&gt; e.Description)));};}):&#160;Program.cs'],['../_program_8cs.html#a21c27872755e13081d1db2efaf4689f0',1,'MapPost(&quot;/user/login&quot;, async(HttpContext context, UserManager UserManager, SignInManager signInManager)=&gt; { var username=context.Request.Form[&quot;username&quot;];var password=context.Request.Form[&quot;password&quot;];if(StringValues.IsNullOrEmpty(username)||StringValues.IsNullOrEmpty(password)) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync(&quot;Log in failed!&quot;);return;} var user=await UserManager.FindByNameAsync(username);if(user==null) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync(&quot;Log in failed!&quot;);return;} var result=await signInManager.PasswordSignInAsync(user, password, false, false);if(result.Succeeded) { context.Response.StatusCode=StatusCodes.Status200OK;context.Response.Redirect(&quot;/user&quot;);} else { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync(&quot;Log in failed!&quot;);} }):&#160;Program.cs'],['../_program_8cs.html#ae7d912025db8547b4d7b1e071713f133',1,'MapPost(&quot;/user/logout&quot;, async(HttpContext context, UserManager userManager, SignInManager signInManager)=&gt; { if(context.User.Identity==null||!context.User.Identity.IsAuthenticated) { context.Response.StatusCode=StatusCodes.Status400BadRequest;return &quot;Already logged out!&quot;;} var user=await userManager.GetUserAsync(context.User);if(user==null) { context.Response.StatusCode=StatusCodes.Status400BadRequest;return &quot;Already logged out!&quot;;} await signInManager.SignOutAsync();return &quot;Logged out successfully!&quot;;}):&#160;Program.cs'],['../_program_8cs.html#ae8308c6d187a2ee938790a7066b93447',1,'MapPost(&quot;/user/update&quot;, async(HttpContext context, UserManager userManager, DbContext dbContext)=&gt; { if(context.User.Identity==null||!context.User.Identity.IsAuthenticated) { context.Response.StatusCode=StatusCodes.Status403Forbidden;await context.Response.WriteAsync($&quot;User update failed!&quot;);return;} var user=await userManager.GetUserAsync(context.User);if(user==null) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync($&quot;User update failed!&quot;);return;} var username=context.Request.Form[&quot;username&quot;];if(StringValues.IsNullOrEmpty(username)) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync(&quot;User update failed!&quot;);return;} else if(username==user.UserName||dbContext.Users.Where(u=&gt; u.UserName==username).Count() &gt; 0) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync(&quot;User update failed!&quot;);return;} user.UserName=username;dbContext.Update(user);await dbContext.SaveChangesAsync();await context.Response.WriteAsync(&quot;Username successfully updated!&quot;);}):&#160;Program.cs'],['../_program_8cs.html#a3b11cc6ba5b37f63a1a74cff86a73a69',1,'MapPost(&quot;/user/delete&quot;, async(HttpContext context, UserManager userManager, DbContext dbContext)=&gt; { if(context.User.Identity==null||!context.User.Identity.IsAuthenticated) { context.Response.StatusCode=StatusCodes.Status403Forbidden;await context.Response.WriteAsync($&quot;User delete failed!&quot;);return;} var user=await userManager.GetUserAsync(context.User);if(user==null) { context.Response.StatusCode=StatusCodes.Status403Forbidden;await context.Response.WriteAsync($&quot;User delete failed!&quot;);return;} await dbContext.Entry(user).Collection(u=&gt; u.Pictures).LoadAsync();var result=await userManager.DeleteAsync(user);if(result.Succeeded) { await dbContext.SaveChangesAsync();await context.Response.WriteAsync(&quot;User deleted Successfully!&quot;);} else { context.Response.StatusCode=400;await context.Response.WriteAsync(&quot;User deletion failed:\n\t - &quot;+string.Join(&quot;\n\t - &quot;, result.Errors.Select(e=&gt; e.Description)));};}):&#160;Program.cs'],['../_program_8cs.html#a30bd0f3af6d42569507b1ab6795afeb9',1,'MapPost(&quot;/photos/{photoName}/create&quot;, async(string photoName, HttpContext context, UserManager userManager, DbContext dbContext)=&gt; { if(context.User.Identity==null||!context.User.Identity.IsAuthenticated) { context.Response.StatusCode=StatusCodes.Status403Forbidden;await context.Response.WriteAsync($&quot;Photo creation failed!&quot;);return;} var user=await userManager.GetUserAsync(context.User);if(user==null) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync($&quot;Photo creation failed!&quot;);return;} if(context.Request.ContentLength==null||context.Request.ContentType==null) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync($&quot;Photo creation failed!&quot;);return;} using(var ms=new MemoryStream()) { await context.Request.Body.CopyToAsync(ms);var content=ms.ToArray();Photo pic=new Photo(user, photoName, content, context.Request.ContentType);dbContext.Add(pic);user.Pictures.Add(pic);await dbContext.SaveChangesAsync();await context.Response.WriteAsync($&quot;Photo {photoName} successfully created!&quot;);} }):&#160;Program.cs'],['../_program_8cs.html#ac7bf11b36845cb42b98150d78f224961',1,'MapPost(&quot;/photos/{photoID}/update&quot;, async(int photoID, HttpContext context, UserManager userManager, DbContext dbContext)=&gt; { if(context.User.Identity==null||!context.User.Identity.IsAuthenticated) { context.Response.StatusCode=StatusCodes.Status403Forbidden;await context.Response.WriteAsync($&quot;Photo update forbidden!&quot;);return;} var user=await userManager.GetUserAsync(context.User);if(user==null) { context.Response.StatusCode=StatusCodes.Status403Forbidden;await context.Response.WriteAsync($&quot;Photo update forbidden!&quot;);return;} if(context.Request.ContentLength==null||context.Request.ContentType==null) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync($&quot;Photo update failed!&quot;);return;} var pic=await dbContext.Pictures.Where(p=&gt; p.Id==photoID).FirstOrDefaultAsync();if(pic==null) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync($&quot;Photo does not exist!&quot;);return;} if(pic.UserId !=user.Id) { context.Response.StatusCode=StatusCodes.Status403Forbidden;await context.Response.WriteAsync($&quot;Photo update forbidden!&quot;);return;} using(var ms=new MemoryStream()) { await context.Request.Body.CopyToAsync(ms);pic.Content=ms.ToArray();pic.ContentType=context.Request.ContentType;dbContext.Update(pic);await dbContext.SaveChangesAsync();await context.Response.WriteAsync($&quot;Photo {pic.FileName} successfully updated!&quot;);} }):&#160;Program.cs'],['../_program_8cs.html#a1abf692cc4c9ea3f246c78f6f7bbd307',1,'MapPost(&quot;/photos/{photoID}/delete&quot;, async(int photoID, HttpContext context, UserManager userManager, DbContext dbContext)=&gt; { if(context.User.Identity==null||!context.User.Identity.IsAuthenticated) { context.Response.StatusCode=StatusCodes.Status403Forbidden;await context.Response.WriteAsync($&quot;Photo deletion failed!&quot;);return;} var user=await userManager.GetUserAsync(context.User);if(user==null) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync($&quot;Photo deletion failed!&quot;);return;} var photos=await dbContext.Pictures.Where(p=&gt; p.Id==photoID).ToListAsync();if(photos.Count==0) { context.Response.StatusCode=StatusCodes.Status400BadRequest;await context.Response.WriteAsync($&quot;Photo {photoID} deletion failed!&quot;);return;} else if(photos.Count &gt; 1||photos[0].UserId !=user.Id) { context.Response.StatusCode=StatusCodes.Status403Forbidden;await context.Response.WriteAsync($&quot;Photo {photoID} deletion forbidden!&quot;);return;} dbContext.Pictures.RemoveRange(photos);await dbContext.SaveChangesAsync();await context.Response.WriteAsync($&quot;Photo {photoID} deleted!&quot;);}):&#160;Program.cs']]]
];
